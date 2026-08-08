# Build, Run and Deploy

From `dotnet build` to a hardened deployment, plus an honest account of what is not production-ready
yet.

- [Build](#build)
- [Run locally](#run-locally)
- [Docker image](#docker-image)
- [Docker Compose](#docker-compose)
- [Configuration and secrets](#configuration-and-secrets)
- [Database migrations as a release step](#database-migrations-as-a-release-step)
- [Scaling out](#scaling-out)
- [Health checks and probes](#health-checks-and-probes)
- [Deploying the Angular client](#deploying-the-angular-client)
- [Deploying the POS client](#deploying-the-pos-client)
- [Production readiness checklist](#production-readiness-checklist)
- [What is still missing for production](#what-is-still-missing-for-production)
- [CI](#ci)

---

## Build

```bash
# Debug
dotnet build src/server/FluentPOS.sln

# Release
dotnet build src/server/FluentPOS.sln -c Release

# Publish a self-contained folder
dotnet publish src/server/API/Bootstrapper.csproj -c Release -o ./publish
```

The entry assembly is `FluentPOS.Bootstrapper.dll`.

Expect **0 errors and roughly 85 StyleCop warnings** — mostly `SA1518` (missing trailing newline) in
files added during Phases 1–4. `TreatWarningsAsErrors` is off (`src/server/Directory.Build.props`).
They are noise, not risk, but they are also easy to clear if you want a clean build.

`src/server/API/PosClient/**` is copied to the output by `Bootstrapper.csproj`, so the PWA ships with
the API automatically.

---

## Run locally

```bash
dotnet run   --project src/server/API      # http://localhost:5000  https://localhost:5001
dotnet watch run --project src/server/API  # with hot reload
```

Ports come from `src/server/API/Properties/launchSettings.json`. Override with
`ASPNETCORE_URLS=http://+:8080`.

Full local setup: [getting-started.md](getting-started.md).

---

## Docker image

`src/server/Dockerfile` is a two-stage build:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build      # restore + publish
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime # + curl for the health check
ENV ASPNETCORE_URLS=http://+:5000
HEALTHCHECK CMD curl --fail http://localhost:5000/health/live
ENTRYPOINT ["dotnet", "FluentPOS.Bootstrapper.dll"]
```

Build and run standalone:

```bash
docker build -t fluentpos-api ./src/server

docker run --rm -p 5000:5000 \
  -e PersistenceSettings__ConnectionStrings__postgres="Host=host.docker.internal;Database=fluentpos;Username=postgres;Password=secret" \
  -e JwtSettings__Key="a-32-plus-character-random-secret-value" \
  -e CorsSettings__Url="http://localhost:4200" \
  fluentpos-api
```

Note the build context is `./src/server`, not the repo root — the Dockerfile does `COPY . .` and
expects the solution at the root of the context.

**Container hardening not yet done:** the image runs as root, has no `.dockerignore` (so `bin`/`obj`
are copied into the build context), and does not pin base-image digests.

---

## Docker Compose

`docker-compose.yml` at the repo root: API + PostgreSQL 16 + Redis 7, with named volumes and a
Postgres healthcheck gating API startup.

```bash
cp .env.example .env      # then set JWT_KEY — compose refuses to start without it
docker compose up --build -d
docker compose logs -f api
docker compose down       # -v also deletes the data volumes
```

`.env` keys:

| Key | Default | Notes |
|---|---|---|
| `JWT_KEY` | **none — required** | 32+ random characters |
| `POSTGRES_PASSWORD` | `postgres` | Change it |
| `CORS_URL` | `http://localhost:4200` | The client origin |

What compose sets on the API:

```yaml
ASPNETCORE_ENVIRONMENT: Production
PersistenceSettings__MigrateOnStartup: "true"    # fine for one replica only
PersistenceSettings__SeedOnStartup: "true"       # sample data — turn off for real use
CacheSettings__UseRedis: "true"
JwtSettings__RequireHttpsMetadata: "false"       # no TLS inside the compose network
```

Only the API publishes a port (5000). Postgres and Redis are internal — add a `ports:` mapping if you
want to attach a client from the host.

**This compose file is a development and demo convenience, not a production topology.** It has no TLS,
an unauthenticated Hangfire dashboard, seeding on, and migration-on-startup.

---

## Configuration and secrets

Every `appsettings.json` key is overridable by environment variable using `__` for nesting:

```bash
PersistenceSettings__ConnectionStrings__postgres="Host=…"
PersistenceSettings__MigrateOnStartup=false
PersistenceSettings__SeedOnStartup=false
JwtSettings__Key="…"
JwtSettings__RequireHttpsMetadata=true
CacheSettings__UseRedis=true
CacheSettings__RedisConnectionString="redis:6379"
CorsSettings__Url="https://pos.example.com"
ApplicationSettings__ApiUrl="https://api.example.com/"
```

Full table: [architecture.md](architecture.md#configuration-reference).

**Secrets that must not stay at their defaults:**

| Setting | Why |
|---|---|
| `JwtSettings.Key` | The sample value is committed to this repo. Anyone can forge tokens |
| `PersistenceSettings.ConnectionStrings.postgres` | Contains a password |
| `MailSettings` | Ships with sample Ethereal credentials |
| `SmsSettings` | Placeholder Twilio values |
| Seeded user passwords | `123Pa$$word!` is public |

Locally, prefer `dotnet user-secrets` over editing the tracked file:

```bash
cd src/server/API
dotnet user-secrets init
dotnet user-secrets set "PersistenceSettings:ConnectionStrings:postgres" "Host=localhost;…"
dotnet user-secrets set "JwtSettings:Key" "…"
```

In a real deployment use your platform's secret store (Azure Key Vault, AWS Secrets Manager,
Kubernetes Secrets) injected as environment variables.

---

## Database migrations as a release step

With `MigrateOnStartup: true` the API migrates on boot. That is convenient with one replica and
**unsafe with more than one** — concurrent migrators race on the same schema.

For anything beyond a single container:

```jsonc
"PersistenceSettings": { "MigrateOnStartup": false, "SeedOnStartup": false }
```

and run migrations as a discrete release step before the new version starts serving:

```powershell
./migrate-database.ps1
# or against a specific startup project
./migrate-database.ps1 -StartupProject "src/server/API"
```

The script walks all nine DbContexts in dependency order and exits non-zero if any fail, so it works
as a pipeline gate. It needs the .NET SDK and `dotnet-ef` — so either a build agent, or a dedicated
migration job image, or generate idempotent SQL and apply that:

```bash
cd src/server/Modules/Catalog/Modules.Catalog.Infrastructure
dotnet ef migrations script --idempotent --context CatalogDbContext \
  --startup-project ../../../API --output catalog.sql
```

Repeat per context; apply the scripts in the order `migrate-database.ps1` uses.

Because there is no production data anywhere yet, migration history was squashed to a single
`Initial` per context in August 2026. **There is no upgrade path from a pre-Phase-1 database** — new
environments bootstrap from empty.

---

## Scaling out

What already works and what does not, if you put more than one API instance behind a load balancer:

| Concern | Status |
|---|---|
| Stateless request handling | ✅ JWT bearer, no server session |
| Distributed cache | ✅ Set `CacheSettings__UseRedis=true` |
| Migrations | ⚠️ Set `MigrateOnStartup=false` and run as a release step |
| Seeding | ⚠️ Set `SeedOnStartup=false` |
| Hangfire | ⚠️ Every instance runs `AddHangfireServer()`, so **N instances = N schedulers**. Hangfire's distributed locks stop double-execution of a given job, but you should still run the job server in one dedicated instance and disable it elsewhere |
| Event delivery | ❌ In-process MediatR only. Nothing survives a process crash between the commit and the handler — the transactional outbox is the fix |
| Sticky sessions | Not needed |
| File uploads (`/files`) | ❌ Written to local disk. Move to blob storage or a shared volume, or images vanish per-instance |

---

## Health checks and probes

| Endpoint | Checks | Use as |
|---|---|---|
| `/health/live` | Nothing — the process answered | Liveness probe |
| `/health/ready` | `ApplicationDbContext` reachability (tagged `ready`) | Readiness probe |

Kubernetes:

```yaml
livenessProbe:
  httpGet: { path: /health/live, port: 5000 }
  periodSeconds: 30
readinessProbe:
  httpGet: { path: /health/ready, port: 5000 }
  initialDelaySeconds: 20
  periodSeconds: 10
```

Give readiness a generous `initialDelaySeconds` if `MigrateOnStartup` is on — the first boot migrates
nine contexts and seeds before serving. The Dockerfile's own healthcheck already allows a 90-second
start period.

Redis is **not** in the readiness check even when enabled. Add it if a Redis outage should take an
instance out of rotation.

---

## Deploying the Angular client

```bash
cd src/client
npm ci
NODE_OPTIONS=--openssl-legacy-provider npx ng build --prod
# output: src/client/dist/
```

Set the API base URL in `src/environments/environment.prod.ts` before building, and make sure the
API's `CorsSettings.Url` matches the origin you serve from — **CORS allows exactly one origin**.

Serve `dist/` from any static host with SPA fallback (all unknown paths → `index.html`).

Reminder: this client is Angular 12 (EOL, TSLint, Protractor) and covers only the pre-multi-store
feature set. Replacing it is a Phase 5 decision — see
[EPOS_TRANSFORMATION_PLAN.md](../EPOS_TRANSFORMATION_PLAN.md).

---

## Deploying the POS client

Nothing to do — `src/server/API/PosClient/**` is copied into the publish output and served at `/pos`
whenever that folder exists next to the running assembly.

Two things to get right in production:

1. **Serve it over HTTPS.** Service workers do not register on plain HTTP (except `localhost`), so
   without TLS you lose offline capability entirely — the whole point of the client.
2. **Cache-bust `sw.js`.** A stale service worker will keep serving an old app shell. Ensure your
   reverse proxy does not add long cache headers to it.

To host it separately instead, copy the folder to a static host and set the `API` constant at the top
of `app.js` to the absolute API base — then add that origin to `CorsSettings.Url`.

---

## Production readiness checklist

Work through this before exposing anything.

**Secrets and auth**
- [ ] `JwtSettings__Key` replaced with 32+ random characters from a secret store
- [ ] `JwtSettings__RequireHttpsMetadata=true`
- [ ] Seeded users deleted or repassworded; `SeedOnStartup=false`
- [ ] Database credentials from a secret store, not `appsettings.json`
- [ ] `MailSettings` / `SmsSettings` replaced with real providers

**Network**
- [ ] TLS terminated at a reverse proxy / ingress; HTTP redirects to HTTPS
- [ ] `CorsSettings__Url` set to the real client origin (one origin only)
- [ ] **`/jobs` protected or blocked.** The Hangfire dashboard has no authorization filter — add one via `DashboardOptions.Authorization`, or block the path at the proxy
- [ ] `/swagger` reviewed — decide whether an OpenAPI document should be public
- [ ] `AllowedHosts` narrowed from `*`
- [ ] **`POST /identity/register` restricted.** It is anonymous and auto-assigns the `Staff` role, so
      anyone who can reach the API can mint a working account with the POS permission set

**Data**
- [ ] `MigrateOnStartup=false`; migrations run as a release step
- [ ] Automated Postgres backups with a tested restore
- [ ] `/files` uploads moved off local disk if running >1 instance

**Operations**
- [ ] Liveness/readiness probes wired
- [ ] Serilog sinks pointed at real log aggregation
- [ ] Redis enabled and monitored if >1 instance
- [ ] Hangfire job server confined to one instance
- [ ] Container runs as a non-root user; base images pinned

---

## What is still missing for production

Stated plainly so nobody discovers it during a rollout.

1. **No transactional outbox.** Events are in-process. A crash between commit and handler loses the
   reporting projection for that sale. Blocks webhooks and partner APIs entirely. **Highest priority.**
2. **Hangfire dashboard is unauthenticated.**
3. **No OpenTelemetry.** No traces or metrics; per-store dashboards are not available.
4. **No integration tests.** Nothing covers HTTP + EF query filters + permissions together, so
   tenancy regressions would not be caught by CI.
5. **No card-payment integration.** `PaymentType` is recorded but no terminal (Dojo, SumUp, Adyen) is
   wired up. On the critical path for a real store; out of scope of this codebase today.
6. **No rate limiting or request-size limits.**
7. **File uploads are local-disk only.**
8. **Angular client is EOL** and has no UI for stores, purchasing, till sessions, refunds or reporting.
9. **No tenant bootstrap path** with seeding off — you get no organization and no default store, and
   store-scoped inserts need one.
10. **Accounting module is an empty shell** in the solution.

Sequenced backlog: [EPOS_TRANSFORMATION_PLAN.md](../EPOS_TRANSFORMATION_PLAN.md).

---

## CI

| Workflow | Trigger | Does |
|---|---|---|
| `.github/workflows/dotnet.yml` | push/PR to `master` | `dotnet restore` → `build -c Release` → `test -c Release` on .NET 10 |
| `.github/workflows/angular.yml` | push/PR to `master`, manual | `npm install` → `npm run build` on Node 14 |
| `.github/workflows/codeql-analysis.yml` | scheduled/PR | CodeQL security scan |
| `.github/workflows/sonarqube.yml` | — | SonarQube analysis |

There is **no** workflow that builds or publishes the Docker image, and no deployment pipeline. Both
are worth adding: build the image on every push to `master`, run `migrate-database.ps1` as a release
gate, then roll the API.
