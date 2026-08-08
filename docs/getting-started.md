# Getting Started

Everything a new developer needs to get FluentPOS running locally, log in, and see data.
Budget ~20 minutes for the local path, ~5 for the Docker path.

- [1. Prerequisites](#1-prerequisites)
- [2. Choose a path](#2-choose-a-path)
- [3. Path A — run locally (recommended for development)](#3-path-a--run-locally-recommended-for-development)
- [4. Path B — run with Docker Compose](#4-path-b--run-with-docker-compose)
- [5. What you get once it is running](#5-what-you-get-once-it-is-running)
- [6. Log in and get a token](#6-log-in-and-get-a-token)
- [7. Run the Angular back-office client](#7-run-the-angular-back-office-client)
- [8. Run the offline-first POS client (PWA)](#8-run-the-offline-first-pos-client-pwa)
- [9. Build and test commands](#9-build-and-test-commands)
- [10. Where to go next](#10-where-to-go-next)

---

## 1. Prerequisites

| Tool | Version | Needed for | Notes |
|---|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | **10.0** | API | `dotnet --version` should print `10.x` |
| [PostgreSQL](https://www.postgresql.org/download/) | 14+ (16 used in CI/compose) | API | Or use the Docker path, which bundles it |
| `dotnet-ef` CLI | 10.x | Migrations | `dotnet tool install --global dotnet-ef` |
| [Node.js](https://nodejs.org/) | **14 or 16** | Angular client only | The Angular app is v12 and will not build on modern Node — see [§7](#7-run-the-angular-back-office-client) |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | any recent | Docker path only | |
| Redis | 7 | Optional | Only if you set `CacheSettings.UseRedis = true`; compose provides it |

The **offline-first POS client needs no toolchain at all** — it is dependency-free JavaScript served
by the API itself at `/pos`.

Editor: Visual Studio 2022+, Rider, or VS Code. A VS Code workspace is provided at
`workspace/fluentpos.code-workspace`.

---

## 2. Choose a path

| | Path A — local | Path B — Docker Compose |
|---|---|---|
| Best for | Day-to-day development, debugging, EF migrations | Trying the system out, demoing, CI-like runs |
| You must install | .NET 10 SDK + PostgreSQL | Docker only |
| Hot reload | Yes (`dotnet watch`) | No (rebuild the image) |
| Postgres reachable from host | Yes | **No** — compose does not publish a port for it |
| Redis | Off by default | On |

---

## 3. Path A — run locally (recommended for development)

### 3.1 Create the database user/server

You only need a running PostgreSQL server — **you do not need to create the `fluentpos` database
yourself**. EF Core creates it on first run.

### 3.2 Point the API at your PostgreSQL

Open `src/server/API/appsettings.json` and set the connection string under
`PersistenceSettings.ConnectionStrings.postgres`:

```jsonc
"PersistenceSettings": {
  "UseMsSql": false,
  "UsePostgres": true,
  "MigrateOnStartup": true,   // applies EF migrations for every module on boot
  "SeedOnStartup": true,      // inserts roles, users, stores, products…
  "connectionStrings": {
    "postgres": "Host=localhost;Database=fluentpos;Username=postgres;Password=<your-password>;Include Error Detail=true"
  }
}
```

> **Do not commit your password.** For local work prefer an environment variable or
> `dotnet user-secrets` instead of editing the tracked file — any config key can be overridden with
> a double-underscore env var:
>
> ```bash
> # bash / Git Bash
> export PersistenceSettings__ConnectionStrings__postgres="Host=localhost;Database=fluentpos;Username=postgres;Password=secret"
> ```
> ```powershell
> # PowerShell
> $env:PersistenceSettings__ConnectionStrings__postgres = "Host=localhost;Database=fluentpos;Username=postgres;Password=secret"
> ```

Prefer MSSQL? Set `UseMsSql: true` / `UsePostgres: false` and follow
[api-switching-database-provider-tutorial.md](api-switching-database-provider-tutorial.md).
PostgreSQL is the supported default and the only provider exercised by CI.

### 3.3 Run the API

From the repository root:

```bash
dotnet run --project src/server/API
# or, with hot reload:
dotnet watch run --project src/server/API
```

On first boot the API will:

1. Apply EF Core migrations for **all nine** DbContexts (creating the database and one schema per module).
2. Seed roles, users, organizations, stores, terminals, VAT rates, brands, categories, 42 products,
   customers, and a supplier — see [seed-data.md](seed-data.md).
3. Start the Hangfire server (the replenishment job is registered here).

Watch the console for `Seeded Default SuperAdmin User.` — that means seeding worked.

### 3.4 Confirm it is up

```bash
curl http://localhost:5000/health/live    # -> Healthy   (process is alive)
curl http://localhost:5000/health/ready   # -> Healthy   (database reachable)
```

Then open <http://localhost:5000/swagger>.

---

## 4. Path B — run with Docker Compose

`docker-compose.yml` at the repo root brings up the API, PostgreSQL 16, and Redis 7.

### 4.1 Create your `.env`

```bash
cp .env.example .env
```

Fill it in — **`JWT_KEY` has no default and compose will refuse to start without it**:

```dotenv
# 32+ characters. Generate one, do not reuse the sample from appsettings.json.
JWT_KEY=change-me-to-a-random-32-plus-character-string
POSTGRES_PASSWORD=postgres
CORS_URL=http://localhost:4200
```

Generate a key:

```bash
openssl rand -base64 48
```
```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Max 256 }))
```

`.env` is gitignored. Never commit it.

### 4.2 Bring the stack up

```bash
docker compose up --build          # add -d to detach
docker compose logs -f api         # watch migration + seeding
docker compose down                # stop (keeps volumes)
docker compose down -v             # stop and delete the database
```

The API is published on <http://localhost:5000>. Postgres and Redis are reachable only from inside
the compose network — add a `ports:` mapping to `docker-compose.yml` if you want to attach a DB
client from your host.

In compose the API runs with `ASPNETCORE_ENVIRONMENT=Production`, `MigrateOnStartup=true`,
`SeedOnStartup=true`, `CacheSettings__UseRedis=true`, and no TLS (terminate TLS at a reverse proxy).
See [deployment.md](deployment.md) before using any of this beyond a demo.

---

## 5. What you get once it is running

| URL | What it is | Auth |
|---|---|---|
| `/swagger` | OpenAPI UI for v1 and v2 | Public UI; endpoints need a bearer token |
| `/api/v1/...` | The REST API (all routes lowercase) | JWT bearer |
| `/pos` | Offline-first till client (PWA) | Signs in with email + password |
| `/jobs` | Hangfire dashboard (recurring replenishment job) | **Unauthenticated — see [deployment.md](deployment.md)** |
| `/health/live` | Liveness (no dependency checks) | Public |
| `/health/ready` | Readiness (includes a DbContext check) | Public |
| `/files` | Static uploads (product images) | Public |

Local ports come from `src/server/API/Properties/launchSettings.json`:
`http://localhost:5000` and `https://localhost:5001`. In Docker: `http://localhost:5000` only.

---

## 6. Log in and get a token

Every non-anonymous endpoint requires `Authorization: Bearer <token>`. Tokens are claim-scoped —
they carry the caller's role permissions plus their `storeId` / `orgId`, which silently filters what
the API returns. See [users-and-access.md](users-and-access.md) for the full model.

### Seeded logins

| Email | Password | Role | Scope |
|---|---|---|---|
| `superadmin@fluentpos.com` | `123Pa$$word!` | SuperAdmin | Head office — every permission, sees all stores |
| `staff@fluentpos.com` | `123Pa$$word!` | Staff | Scoped to **Store One** |
| `franchisee@fluentpos.com` | `123Pa$$word!` | Manager | Scoped to the **Northern Franchise Ltd** organization |

### curl

```bash
curl -s -X POST http://localhost:5000/api/v1/identity/tokens \
  -H "Content-Type: application/json" \
  -d '{"email":"superadmin@fluentpos.com","password":"123Pa$$word!"}'
```

Response: `{ "token": "...", "refreshToken": "...", "refreshTokenExpiryTime": "..." }`.
Access tokens last 60 minutes; refresh tokens 7 days (`JwtSettings` in `appsettings.json`).

Save it for reuse:

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/identity/tokens \
  -H "Content-Type: application/json" \
  -d '{"email":"superadmin@fluentpos.com","password":"123Pa$$word!"}' \
  | python -c "import sys,json;print(json.load(sys.stdin)['token'])")

curl -s "http://localhost:5000/api/v1/catalog/products?pageNumber=1&pageSize=5" \
  -H "Authorization: Bearer $TOKEN"
```

### PowerShell

```powershell
$body = @{ email = 'superadmin@fluentpos.com'; password = '123Pa$$word!' } | ConvertTo-Json
$auth = Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/v1/identity/tokens `
        -ContentType 'application/json' -Body $body
$H = @{ Authorization = "Bearer $($auth.token)" }

Invoke-RestMethod -Uri 'http://localhost:5000/api/v1/catalog/products?pageNumber=1&pageSize=5' -Headers $H
```

### Swagger UI

Click **Authorize**, paste the raw token (Swagger adds the `Bearer ` prefix), and every
*Try it out* call is authenticated.

### Postman

`postman/` contains a collection. Note it predates the multi-store work, so store/till/purchasing/
reporting endpoints are missing — Swagger is the current source of truth.

---

## 7. Run the Angular back-office client

> **Heads up.** The client is Angular 12, which is end-of-life. It covers the *original* FluentPOS
> feature set only (products, brands, categories, customers, orders, users, roles) — it has **no UI**
> for stores, terminals, store-product overlays, suppliers, purchase orders, replenishment, till
> sessions, refunds, reporting, or royalties. Those are API-only today. Use Swagger or the
> [testing guide](testing-guide.md) to exercise them. Replacing this client is a Phase 5 item.

```bash
cd src/client
npm install
npm run start        # http://localhost:4200
```

The `npm` scripts use Windows `set NODE_OPTIONS=--openssl-legacy-provider && ...` syntax. On
Linux/macOS/Git Bash, run the equivalent yourself:

```bash
NODE_OPTIONS=--openssl-legacy-provider npx ng serve
```

The client calls `https://localhost:5001/api/v1/` (`src/client/src/environments/environment.ts`).
Two consequences when running the API locally:

- Use the **HTTPS** endpoint, and accept the dev certificate once (`dotnet dev-certs https --trust`).
- If you run the API in Docker (HTTP on port 5000), edit `environment.ts` to
  `http://localhost:5000/api/v1/`.

CORS allows exactly one origin — `CorsSettings.Url`, default `http://localhost:4200`.

Other client commands: `npm run build`, `npm test` (Karma/Jasmine), `npm run lint` (TSLint),
`npm run e2e` (Protractor).

---

## 8. Run the offline-first POS client (PWA)

Nothing to install. With the API running, open <http://localhost:5000/pos>.

1. Sign in with `staff@fluentpos.com` / `123Pa$$word!`. Because that user is scoped to Store One,
   the token, prices, and sales are all Store One's.
2. The client pulls the catalog from `GET /api/v1/catalog/sync` and caches products, store price
   overlays, and VAT rates in IndexedDB.
3. Tap products to build a basket — the basket lives **on the device**, not the server.
4. Checkout posts a complete sale document to `POST /api/v1/sales/orders/pos`. The device-generated
   `clientSaleId` *is* the order id, so replaying a queued sale never double-charges.
5. Kill the API and reload the page: the service worker serves the shell and the cached catalog, and
   sales queue in a durable IndexedDB outbox. Restart the API and the outbox drains automatically
   (on the `online` event, plus a 15-second retry).

Known gaps: it signs in with email/password rather than the device-key + PIN flow (which the API
supports — see [users-and-access.md](users-and-access.md#terminal-device-auth-and-pin-sign-in));
deleted products linger in device caches until a full resync (no tombstones); and permanently
invalid queued sales are dropped client-side.

Source: `src/server/API/PosClient/` (`index.html`, `app.js`, `sw.js`, `manifest.json`), copied to
the publish output by `Bootstrapper.csproj`.

---

## 9. Build and test commands

```bash
# Server
dotnet build src/server/FluentPOS.sln          # expect 0 errors (~85 StyleCop warnings are known)
dotnet test  src/server/FluentPOS.sln          # 33 unit tests across 4 test projects
dotnet run   --project src/server/API
dotnet watch run --project src/server/API

# Client (from src/client/)
npm install && npm run start
```

Migrations are applied automatically when `PersistenceSettings.MigrateOnStartup` is `true`. To apply
them by hand — required when you turn that flag off, which you should for multi-replica deployments:

```powershell
./migrate-database.ps1
```

That script walks all nine DbContexts. To add a migration for one module:

```bash
cd src/server/Modules/Catalog/Modules.Catalog.Infrastructure
dotnet ef migrations add <MigrationName> --context CatalogDbContext --startup-project ../../../API
```

Full context list and details: [architecture.md](architecture.md#persistence-one-database-nine-contexts).

---

## 10. Where to go next

| If you want to… | Read |
|---|---|
| Understand how the modules fit together | [architecture.md](architecture.md) |
| Find an endpoint | [api-reference.md](api-reference.md) |
| Understand roles, permissions, store scoping; add users | [users-and-access.md](users-and-access.md) |
| Know exactly what is in the database after seeding | [seed-data.md](seed-data.md) |
| Walk a full retail scenario end to end | [testing-guide.md](testing-guide.md) |
| Deploy this somewhere real | [deployment.md](deployment.md) |
| Fix a startup error | [troubleshooting.md](troubleshooting.md) |
| See what is built and what is next | [../EPOS_TRANSFORMATION_PLAN.md](../EPOS_TRANSFORMATION_PLAN.md) |
| Learn the domain vocabulary | [../UBIQUITOUS_LANGUAGE.md](../UBIQUITOUS_LANGUAGE.md) |
