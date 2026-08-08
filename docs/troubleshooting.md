# Troubleshooting

Failures you are likely to hit, in roughly the order a newcomer hits them.

- [Startup and database](#startup-and-database)
- [Authentication and authorization](#authentication-and-authorization)
- [Tenancy: "my data disappeared"](#tenancy-my-data-disappeared)
- [Migrations](#migrations)
- [Docker Compose](#docker-compose)
- [Angular client](#angular-client)
- [POS client (PWA)](#pos-client-pwa)
- [Build warnings and analyzers](#build-warnings-and-analyzers)
- [Known landmines when writing code](#known-landmines-when-writing-code)
- [Getting more detail out of the system](#getting-more-detail-out-of-the-system)

---

## Startup and database

### `Npgsql.NpgsqlException: Failed to connect to 127.0.0.1:5432`

PostgreSQL is not running or not listening where you think.

```bash
pg_isready -h localhost -p 5432
```
```powershell
Get-Service postgresql*
```

Then verify `PersistenceSettings.ConnectionStrings.postgres` in `src/server/API/appsettings.json`.
Note the **committed default password is `Zaq1Xsw2`**, which is almost certainly not yours.

### `28P01: password authentication failed for user "postgres"`

Wrong password. Override without editing the tracked file:

```bash
export PersistenceSettings__ConnectionStrings__postgres="Host=localhost;Database=fluentpos;Username=postgres;Password=<yours>"
```

### `3D000: database "fluentpos" does not exist`

You should not need to create it — EF does, when `MigrateOnStartup` is `true`. If that flag is off,
either turn it on for local work or run `./migrate-database.ps1`.

### The API starts but there is no data

Check `PersistenceSettings.SeedOnStartup`. If it is `true` and you still have nothing, look for
`An error occurred while seeding <Module> data.` in the console — seeders catch and log rather than
crash, so a failed seed is easy to miss.

Also remember seeders are **skip-if-not-empty**. If a table has any rows, the seeder does nothing at
all. Reset it: [seed-data.md](seed-data.md#resetting-the-database).

### `Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone'`

Something persisted `DateTime.Now`. Every stored timestamp must be `DateTime.UtcNow`. This class of
bug was fixed across Order/Cart/Stock/StockTransaction/EntityReference in Phase 0 — do not
reintroduce it.

### First boot is slow

It migrates nine DbContexts then seeds 42 products, 20 customers, brands, categories, roles and
permission claims. Twenty to sixty seconds is normal. Subsequent boots are fast.

---

## Authentication and authorization

### 401 on every call

- Missing `Authorization: Bearer <token>` header.
- Token expired — access tokens live 60 minutes. Get a new one, or use
  `POST /identity/tokens/refresh` (which accepts an expired access token plus a valid refresh token).
- `JwtSettings.Key` changed since the token was issued. Any key change invalidates all live tokens.
- Issuer/audience mismatch — both are validated. If you overrode one via env var, tokens minted
  before the change fail.

### 403 despite having "the right" role

Permissions are **claims on the role**, not the role name. Three of the six seeded roles —
**Admin, Accountant, Cashier — have no permissions at all.** A user with the Admin role gets 403 on
everything until you grant claims.

Also: claims are baked into the token at issuance. After changing a role's permissions or a user's
roles, **the user must get a new token.**

Check what you actually hold by decoding the token at <https://jwt.io> and reading the `Permission`
claims.

### Staff cannot open a till session or refund

Correct — by design of the seed data. `TillSessions.Open` and `Sales.Refund` are **not** in the
seeded Staff permission set. Use the admin token, or grant the permissions:
[users-and-access.md](users-and-access.md#granting-permissions-to-a-role).

### `POST /identity/register` succeeds but the user cannot sign in

Email confirmation. Pass `emailConfirmed: true` (and `phoneNumberConfirmed: true`) on registration for
local work, or set `MailSettings.EnableVerification: false`.

Two other registration gotchas: `userName` and `password` each need **6+ characters**, and every new
user is auto-assigned the **Staff** role — so they inherit Staff's permissions until you change them.

### PIN sign-in returns unauthorized

- The device key is shown **once** at registration; only a SHA-256 hash is stored. If you lost it,
  re-run `POST /organization/terminals/{id}/register-device` — which **rotates** the key and
  invalidates the old one.
- The user must have set a PIN via `POST /identity/tokens/pin/setup` first.
- A store-scoped user **cannot** sign in at another store's terminal. That is the intended behaviour.

---

## Tenancy: "my data disappeared"

The most confusing category. Symptoms: an entity you know exists returns 404, or a list comes back
shorter than the database.

**This is almost always the global query filter doing its job.** Entities implementing
`IMustHaveStore` are filtered by the `storeId` claim on your token before any handler runs.

Diagnose in order:

1. Decode your token. Does it have a `storeId` claim? If yes, you are store-scoped.
2. Retry the same call with the `superadmin` token (no `storeId` — unscoped). If the data appears,
   it is scoping, not a bug.
3. Check which store the row actually belongs to in the database.

Related behaviours that look like bugs but are not:

| Symptom | Reason |
|---|---|
| 404 instead of 403 for another store's order | The filter removes the row before the handler sees it |
| 403 when touching another store's cart | Write path checks explicitly |
| Head office's sale landed in Store One | Users with no `storeId` transact against the **default store** unless the command names one. Pass `storeId` explicitly |
| A price override was ignored | The `StoreProduct` overlay is per store — check `storeId`, and that `isRanged` is true |
| Reporting shows one row | Store staff see only their store; franchisee managers their organization |

---

## Migrations

### `Unable to create an object of type '<X>DbContext'`

Run `dotnet ef` from the module's **Infrastructure** directory and pass both `--context` and
`--startup-project`:

```bash
cd src/server/Modules/Catalog/Modules.Catalog.Infrastructure
dotnet ef migrations add MyChange --context CatalogDbContext --startup-project ../../../API
```

All nine contexts have a design-time factory, so this works without the host running.

### `No DbContext was found` / `More than one DbContext was found`

`--context` is mandatory — there are nine in the solution. Names:
`ApplicationDbContext`, `IdentityDbContext`, `OrganizationDbContext` (singular "Organization"),
`CatalogDbContext`, `PeopleDbContext`, `SalesDbContext`, `InventoryDbContext`,
`PurchasingDbContext`, `ReportingDbContext`.

### `dotnet ef` not recognised

```bash
dotnet tool install --global dotnet-ef
dotnet tool update  --global dotnet-ef
```

### `migrate-database.ps1` reports a failed context

It exits non-zero and lists which contexts failed. Run that one context by hand with `--verbose` to
see the real error. A missing project path prints a warning and counts as a failure.

### `relation "..." already exists`

Usually a schema that survived a `__EFMigrationsHistory` reset, or vice versa. Cleanest fix, given
there is no production data anywhere: drop the database and start over
([seed-data.md](seed-data.md#resetting-the-database)).

---

## Docker Compose

### `Set JWT_KEY in .env (32+ characters)`

Working as intended — compose uses `${JWT_KEY:?…}` so it refuses to start without one.
`cp .env.example .env` and fill it in.

### API container restarts in a loop

```bash
docker compose logs api
```

Usually Postgres not ready (the healthcheck should prevent this — check `docker compose ps`), or a
bad connection string, or a migration failure.

### Cannot connect to Postgres from my machine

Deliberate — compose publishes no port for it. Add one:

```yaml
postgres:
  ports:
    - "5432:5432"
```

Or exec in: `docker compose exec postgres psql -U postgres -d fluentpos`.

### Stale data after a schema change

`docker compose down -v` — the `-v` deletes the volumes. Without it the old database persists.

### Rebuilding does not pick up my code change

```bash
docker compose build --no-cache api && docker compose up -d api
```

There is no `.dockerignore`, so a large `bin`/`obj` tree also makes the build context slow to send.

---

## Angular client

### `error:0308010C:digital envelope routines::unsupported`

Angular 12 against modern Node. The npm scripts set `NODE_OPTIONS=--openssl-legacy-provider` using
Windows `set` syntax, which does nothing on Linux/macOS/Git Bash:

```bash
NODE_OPTIONS=--openssl-legacy-provider npx ng serve
```

Better: use Node 14 or 16 (`nvm use 16`).

### `npm install` fails with peer-dependency or engine errors

The lockfile is from the Angular 12 era. Try `npm install --legacy-peer-deps`, and use an older Node.

### CORS errors in the browser console

`CorsSettings.Url` allows exactly **one** origin, default `http://localhost:4200`. If you serve the
client anywhere else, change it (`CorsSettings__Url`) and restart the API.

### `net::ERR_CERT_AUTHORITY_INVALID` calling the API

The client targets `https://localhost:5001`. Trust the dev certificate:

```bash
dotnet dev-certs https --trust
```

Or point `src/environments/environment.ts` at `http://localhost:5000/api/v1/`.

### Store, purchasing, till or reporting screens are missing

They do not exist. The Angular client covers only the pre-multi-store feature set. Use Swagger or the
[testing guide](testing-guide.md).

---

## POS client (PWA)

### `/pos` returns 404

The static file provider is only registered if a `PosClient` folder exists next to the running
assembly. Confirm `src/server/API/PosClient/` is present and rebuild — `Bootstrapper.csproj` copies
it to the output.

### Offline mode does not work

- Service workers require **HTTPS**, except on `localhost`. Over plain HTTP on a real host, offline
  capability is silently unavailable.
- Check DevTools → Application → Service Workers that `sw.js` is registered and activated.
- Load the page online at least once first — there is nothing to cache otherwise.

### A code change to the PWA does not appear

A stale service worker is serving the old shell. DevTools → Application → Service Workers →
*Unregister*, then hard-reload. Or *Clear site data*, which also drops the IndexedDB caches.

### Sales stay stuck in the outbox

The outbox drains on the browser's `online` event and retries every 15 seconds. If sales stay queued:

- The API is unreachable, or the token expired while offline (re-sign-in is needed).
- The sale is being **rejected** rather than failing — permanently invalid sales are dropped
  client-side. Watch the network tab for a 400.

### Products are missing or prices are wrong after an edit

The device caches the catalog and pulls incrementally. There are **no tombstones**, so deleted
products linger until a full resync. Clear site data to force a full pull.

---

## Build warnings and analyzers

`dotnet build` prints roughly **85 warnings, 0 errors** — mostly `SA1518` ("file may not end with a
newline") in files added during Phases 1–4, plus assorted StyleCop and Roslynator notes.

They are not failures: `TreatWarningsAsErrors` is `false` in
`src/server/Directory.Build.props`. Rules live in `src/server/stylecop.json` and
`src/server/fluentpos.ruleset`. Clearing SA1518 is a mechanical, low-risk cleanup if the noise bothers
you.

---

## Known landmines when writing code

Every one of these has already caused a real bug in this repo.

| Do not | Because |
|---|---|
| `DateTime.Now` on anything persisted | PostgreSQL `timestamptz` rejects `Kind=Local`. Always `UtcNow` |
| `OrderBy` **after** `ProjectTo` | Untranslatable; throws at runtime. Order before projecting |
| Reference another module's project | Breaks modularity. Use a `Shared.Core` interface or a MediatR notification |
| Forget `IMustHaveStore` on a store-scoped entity | It silently becomes globally visible — a tenancy leak |
| Forget `ISyncTracked` on catalog-ish entities | POS nodes never see the changes |
| Add a permission constant without wiring `[Authorize(Policy = …)]` | The endpoint stays open |
| Cache a store-scoped response without the store in the key | Cross-tenant read. `CachingBehavior` handles this — do not bypass it |
| `Task.Run` for background work | Use `IJobService` (Hangfire) so it survives a restart |
| Assume an event handler ran | Events are **in-process only**; there is no outbox. A crash loses them |
| Make a controller `public` | Controllers are `internal sealed`, discovered by `InternalControllerFeatureProvider` |

---

## Getting more detail out of the system

**Turn up logging** in `src/server/API/appsettings.Development.json`:

```jsonc
{ "Logging": { "LogLevel": { "Default": "Debug", "Microsoft.EntityFrameworkCore.Database.Command": "Information" } } }
```

That last one logs every SQL statement — the fastest way to see a global query filter in action.

**Postgres error detail** is already on in the default connection string
(`Include Error Detail=true`), which turns opaque constraint violations into readable messages.

**The event log** records every domain event with user attribution:

```bash
curl -s "$BASE/identity/eventlogs?pageNumber=1&pageSize=20" -H "$AH"
```

Use it to confirm a sale, refund or verification actually fired the event you expected.

**Hangfire dashboard** at <http://localhost:5000/jobs> — see the replenishment job's schedule, history
and failures.

**Swagger** at <http://localhost:5000/swagger> is generated from the code, so when a payload in these
docs disagrees with Swagger, Swagger is right.
