# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Human-facing documentation lives in [`docs/`](docs/README.md)** — architecture, API reference,
users/permissions, seed data, testing scenarios, deployment, troubleshooting. Prefer updating those
docs over duplicating detail here.

## Commands

### Server (ASP.NET Core 10)
```bash
# Build the solution — expect 0 errors, ~85 known StyleCop warnings (SA1518 etc.)
dotnet build src/server/FluentPOS.sln

# Run the 33 unit tests (Sales, Purchasing, Reporting, Shared.Infrastructure)
dotnet test src/server/FluentPOS.sln

# Run the API (http://localhost:5000, https://localhost:5001)
dotnet run --project src/server/API
dotnet watch run --project src/server/API      # hot reload

# Apply migrations for all nine DbContexts (needed when MigrateOnStartup is false)
./migrate-database.ps1
./migrate-database.ps1 -Context CatalogDbContext

# Add an EF migration — --context and --startup-project are both required
cd src/server/Modules/<Name>/Modules.<Name>.Infrastructure
dotnet ef migrations add <MigrationName> --context <Name>DbContext --startup-project ../../../API
```

Contexts: `ApplicationDbContext` (Shared.Infrastructure), `IdentityDbContext`,
`OrganizationDbContext` (singular), `CatalogDbContext`, `PeopleDbContext`, `SalesDbContext`,
`InventoryDbContext`, `PurchasingDbContext`, `ReportingDbContext`.

### Client (Angular 12 — legacy)
```bash
# From src/client/ — needs Node 14/16; will not build on modern Node
npm install
npm run start        # dev server at http://localhost:4200
npm run build        # production build
npm test             # unit tests (Karma/Jasmine)
npm run lint         # TSLint
npm run e2e          # Protractor end-to-end tests
```

The npm scripts use Windows `set NODE_OPTIONS=--openssl-legacy-provider` syntax; on bash use
`NODE_OPTIONS=--openssl-legacy-provider npx ng serve`.

### Docker
```bash
cp .env.example .env      # JWT_KEY is required — compose will not start without it
docker compose up --build -d
docker compose down -v    # also deletes the database volume
```

### Local Configuration
Before running, set `PersistenceSettings.ConnectionStrings.postgres` in
`src/server/API/appsettings.json` (or override with the env var
`PersistenceSettings__ConnectionStrings__postgres`). Any config key is overridable with `__` nesting.

- `MigrateOnStartup` and `SeedOnStartup` both default to `true`: the database is created, migrated and
  seeded on first run.
- Seeded logins, all with password `123Pa$$word!`: `superadmin@fluentpos.com` (SuperAdmin, head
  office), `staff@fluentpos.com` (Staff, scoped to Store One), `franchisee@fluentpos.com` (Manager,
  scoped to the Northern Franchise organization).
- Endpoints: `/swagger`, `/pos` (offline-first PWA till), `/jobs` (Hangfire, **unauthenticated**),
  `/health/live`, `/health/ready`.

## Architecture

FluentPOS is a **modular monolith** — a single deployable API composed of isolated feature modules,
each following Clean Architecture (Onion/Hexagonal). On top of that base this fork adds three-level
tenancy (organization → store → terminal), an event-projected reporting read model, and an
offline-first POS client that treats each store as a node. Full detail:
[docs/architecture.md](docs/architecture.md).

### Modules

| Module | Route prefix | Owns |
|---|---|---|
| Identity | `api/v1/identity/*` | Users, roles, permission claims, JWT, PIN/device sign-in, event logs |
| Organizations | `api/v1/organization/*` | Organizations, stores, terminals, device registration |
| Catalog | `api/v1/catalog/*` | Products, brands, categories, VAT rates, store overlays, sync feed |
| People | `api/v1/people/*` | Customers, carts, cart items |
| Sales | `api/v1/sales/*` | Orders, payment transactions, refunds, till sessions |
| Inventory | — | Per-store stock; no controllers, consumed via `IStockService` |
| Purchasing | `api/v1/purchasing/*` | Suppliers, purchase orders, replenishment, price files |
| Reporting | `api/v1/reporting/*` | `DailyStoreSales` read model, royalty accrual |
| Accounting | — | **Empty shell** — csproj files only, not referenced by the host |

### Module Structure

Every module under `src/server/Modules/<Name>/` has three projects:

| Project | Purpose |
|---|---|
| `Modules.<Name>.Core` | Domain entities, MediatR commands/queries/events, FluentValidation validators, AutoMapper profiles, `I<Name>DbContext` interface |
| `Modules.<Name>.Infrastructure` | EF Core `DbContext`, design-time factory, migrations, seeders, services implementing Core interfaces |
| `Modules.<Name>` | ASP.NET Controllers, module registration extension (`Add<Name>Module`) |

Test projects sit alongside as `Modules.<Name>.Core.Tests` (Sales, Purchasing, Reporting only).

### How Modules Are Wired

`src/server/API/Startup.cs` calls `Add<Name>Module(_config)` for each module in `ConfigureServices`.
Order matters: Identity before `AddSharedApplication`, Organizations before Catalog/Sales.
Each module's `ModuleExtensions.cs` registers its own controllers (via `IMvcBuilder`) and services.
Shared infrastructure (`AddSharedInfrastructure`) sets up middleware, Swagger, JWT, CORS, health
checks, Hangfire, caching, and global exception handling — all modules inherit this.

Controllers are `internal sealed`, discovered by a custom `InternalControllerFeatureProvider`. Do not
make them public.

### Cross-Module Communication Rules

- Modules **cannot directly reference each other's projects** or modify each other's database tables.
- Cross-cutting concerns are handled via **interfaces** in `Shared.Core` (`IStoreService`,
  `IProductService`, `IStoreProductService`, `IStockService`, `ICustomerService`, `ICurrentUser`,
  `ITenantContext`) and **domain events** via MediatR (`INotification` / `INotificationHandler`).
- Shared DTOs live in `Shared.DTOs` and are the only safe way to pass data across module boundaries.

### Multi-Tenancy (read this before touching any entity)

`Organization` → `Store` → `Terminal`. Enforcement:

1. `TokenService` stamps `storeId` and `orgId` claims on the JWT. **No `storeId` = head office
   (unscoped).**
2. `ITenantContext` reads those claims per request.
3. `ModuleDbContext.OnModelCreating` applies a **global query filter** to every entity implementing
   `IMustHaveStore`, and auto-stamps the tenant's store on insert.

Store-scoped: `Stock`, `StockTransaction`, `Order`, `Transaction`, `Cart`, `TillSession`,
`PurchaseOrder`, `StoreProduct`. Centrally owned (org-level): `Product`, `Brand`, `Category`,
`VatRate`, `Customer`, `Supplier`.

**A new store-scoped entity that does not implement `IMustHaveStore` is a tenancy leak.** A
store-scoped user reading another store's entity gets 404 (filtered before the handler); writing
across the boundary gets 403.

### Shared Libraries

- `Shared.Core` — base entity types (`BaseEntity`, `AuditableEntity`), marker interfaces
  (`IMustHaveStore`, `ISyncTracked`), shared service interfaces, `Permissions` constants,
  `OrganizationConstants` (fixed seeded GUIDs), `Result`/`PaginatedResult` wrappers.
- `Shared.Infrastructure` — middleware pipeline, JWT auth, permission policy provider, Swagger,
  AutoMapper, Serilog, global exception handling, `ModuleDbContext` (auditing + event publishing +
  tenancy filters + sync stamping), MediatR behaviours (validation, caching), event logging.
- `Shared.DTOs` — request/response DTOs shared across modules and the API surface.

### Feature Pattern (CQRS + MediatR)

Within each module's Core project, features follow this layout:
```
Features/<Entity>/
  Commands/         # IRequest<Result<T>> commands + IRequestHandler
  Commands/Validators/  # FluentValidation AbstractValidator<TCommand>
  Events/           # INotification domain events + INotificationHandler
  Queries/          # IRequest<T> query records + IRequestHandler
  Queries/Validators/
```

Controllers do nothing but `Mediator.Send(...)`. Pipeline behaviours: validation (failures → 400) and
caching (keys partitioned by the caller's store — do not bypass).

### Authorization

Claim-based fine-grained permissions from `Shared.Core/Constants/Permissions.cs`, enforced by
`PermissionPolicyProvider` + `PermissionAuthorizationHandler` via
`[Authorize(Policy = Permissions.<Group>.<Action>)]`.

Six roles are seeded but **only SuperAdmin (all), Staff (POS subset) and Manager (reporting subset)
receive permissions** — Admin, Accountant and Cashier are name-only. Notably, seeded Staff cannot
open a till session or issue a refund. See [docs/users-and-access.md](docs/users-and-access.md).

### Eventing

Domain events are in-process MediatR notifications published from `SaveChangesAsync`, and every one is
audit-logged to `EventLogs`. **There is no transactional outbox** — nothing survives a crash between
commit and handler. `OrderRegistered` / `OrderRefunded` project into the Reporting module's
`DailyStoreSales`. Adding the outbox is the top backlog item.

Background work goes through `IJobService` (Hangfire), never `Task.Run`.

### Angular Client

The Angular app (`src/client/`) mirrors the module structure under `src/app/modules/` (auth, admin,
home, pos). Core services, guards, and interceptors are in `src/app/core/`. JWT tokens are obtained
from `api/identity/tokens` and attached by an HTTP interceptor.

It is **Angular 12 (EOL)** and covers only the pre-multi-store feature set — no UI for stores,
terminals, store-product overlays, suppliers, purchase orders, replenishment, till sessions, refunds,
reporting or royalties. Those are API-only.

### Offline-First POS Client

`src/server/API/PosClient/` — dependency-free PWA served at `/pos`. Device-owned basket, IndexedDB
catalog cache, durable sale outbox with automatic replay, service worker for a loadable shell with the
server down. Two endpoints make it work: `GET api/v1/catalog/sync?since=<cursor>` (incremental pull,
server-clock cursors) and `POST api/v1/sales/orders/pos` (the device-generated `clientSaleId` **is**
the order id, so replays are idempotent).

## Code Style

- **C#**: StyleCop analyzers enforced via `src/server/stylecop.json` and `src/server/fluentpos.ruleset`
  (`TreatWarningsAsErrors` is off). 4-space indent, PascalCase for types and public members, `I` prefix
  for interfaces. Every file carries the FluentPOS copyright header.
- **TypeScript/Angular**: 2-space indent, camelCase for variables and methods, `*.component.ts` /
  `*.service.ts` naming.
- **Tests**: xUnit + FakeItEasy. Class names `<ThingUnderTest>Should`; method names continue the
  sentence.
- **Commit messages**: Present-tense with scope prefix — e.g., `API: add product search endpoint`,
  `NG: fix cart total calculation`, `docs: update deployment guide`.
- **Branch naming**: `fluentpos-<issueId>` targeting `master`. (Transformation work uses `Phase_NN`.)

## Landmines

Each of these has already caused a real bug here:

- **Never `DateTime.Now` on persisted values** — PostgreSQL `timestamptz` rejects `Kind=Local`. Use
  `UtcNow`.
- **Never `OrderBy` after `ProjectTo`** — untranslatable, throws at runtime. Order before projecting.
- **New store-scoped entity → implement `IMustHaveStore`**, or it is globally visible.
- **New catalog-ish entity → implement `ISyncTracked`**, or POS nodes never see changes.
- **New permission constant → wire `[Authorize(Policy = …)]`**, or the endpoint is open.
- **Do not assume an event handler ran** — in-process only, no outbox.
- Head-office users (no `storeId`) transact against the **default store** unless a command names one.

## Reference

- [docs/README.md](docs/README.md) — documentation index
- [EPOS_TRANSFORMATION_PLAN.md](EPOS_TRANSFORMATION_PLAN.md) — assessment, phase status, and the
  sequenced backlog (§5 is the current plan)
- [UBIQUITOUS_LANGUAGE.md](UBIQUITOUS_LANGUAGE.md) — domain vocabulary; use these words
- `/swagger` — generated from the code, authoritative over any hand-written payload in docs
