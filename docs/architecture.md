# Architecture

How FluentPOS is put together, and the handful of concepts you need before changing anything.

- [The shape in one paragraph](#the-shape-in-one-paragraph)
- [Repository layout](#repository-layout)
- [Modules](#modules)
- [Anatomy of a module](#anatomy-of-a-module)
- [How modules are wired at startup](#how-modules-are-wired-at-startup)
- [Cross-module communication rules](#cross-module-communication-rules)
- [The feature pattern (CQRS + MediatR)](#the-feature-pattern-cqrs--mediatr)
- [Request pipeline](#request-pipeline)
- [Multi-tenancy: organizations, stores, terminals](#multi-tenancy-organizations-stores-terminals)
- [Persistence: one database, nine contexts](#persistence-one-database-nine-contexts)
- [Eventing and the reporting read model](#eventing-and-the-reporting-read-model)
- [The store-node sync protocol](#the-store-node-sync-protocol)
- [Background jobs](#background-jobs)
- [Caching](#caching)
- [Configuration reference](#configuration-reference)
- [Known architectural debt](#known-architectural-debt)

---

## The shape in one paragraph

FluentPOS is a **modular monolith**: one deployable ASP.NET Core API composed of isolated feature
modules, each following Clean Architecture (Onion/Hexagonal). Modules never reference each other's
projects or tables — they talk through interfaces declared in `Shared.Core` and through MediatR
notifications. Persistence is one PostgreSQL database with **one schema per module**. On top of that
base, this fork adds a three-level tenancy model (organization → store → terminal) enforced by EF
Core global query filters, an event-projected reporting read model, and an offline-first POS client
that treats each store as a node.

---

## Repository layout

```
fluentpos/
├── src/
│   ├── server/                       ASP.NET Core solution (FluentPOS.sln)
│   │   ├── API/                      Bootstrapper.csproj — host, Startup, appsettings
│   │   │   └── PosClient/            Offline-first PWA till, served at /pos
│   │   ├── Modules/<Name>/           One folder per module (3 projects each)
│   │   ├── Shared/                   Shared.Core · Shared.DTOs · Shared.Infrastructure (+ tests)
│   │   ├── Directory.Build.props     StyleCop/Roslynator analyzers for every project
│   │   ├── stylecop.json             Style rules
│   │   └── fluentpos.ruleset         Analyzer severities
│   └── client/                       Angular 12 back-office app
├── docs/                             You are here
├── postman/                          API collection (predates multi-store work)
├── workspace/                        VS Code workspace
├── docker-compose.yml                API + Postgres 16 + Redis 7
├── migrate-database.ps1              Applies migrations for all nine DbContexts
└── EPOS_TRANSFORMATION_PLAN.md       Assessment, roadmap, phase status, backlog
```

---

## Modules

| Module | Route prefix | Owns | Status |
|---|---|---|---|
| **Identity** | `api/v1/identity/*` | Users, roles, permission claims, JWT issuance, PIN/device sign-in, event logs | Active |
| **Organizations** | `api/v1/organization/*` | Organization, Store, Terminal; franchisee onboarding; device registration | Active |
| **Catalog** | `api/v1/catalog/*` | Products, brands, categories, VAT rates, per-store product overlays, sync feed | Active |
| **People** | `api/v1/people/*` | Customers, carts, cart items | Active |
| **Sales** | `api/v1/sales/*` | Orders, payment transactions, refunds, till sessions, cash movements | Active |
| **Inventory** | — | Per-store stock and stock transactions | Active (no controllers; consumed via `IStockService`) |
| **Purchasing** | `api/v1/purchasing/*` | Suppliers, purchase orders, receiving, auto-replenishment, wholesaler price files | Active |
| **Reporting** | `api/v1/reporting/*` | `DailyStoreSales` read model, royalty accrual | Active |
| **Accounting** | — | — | **Empty shell.** Three `.csproj` files, zero source, not referenced by the host. |

`Modules.Accounting.*` is in `FluentPOS.sln` but is not a `ProjectReference` of `Bootstrapper.csproj`
and is not registered in `Startup.cs`. It builds to nothing. Either fill it or drop it.

---

## Anatomy of a module

Every module under `src/server/Modules/<Name>/` is three projects:

| Project | Contains | Depends on |
|---|---|---|
| `Modules.<Name>.Core` | Domain entities, MediatR commands/queries/events + handlers, FluentValidation validators, AutoMapper profiles, the `I<Name>DbContext` interface | `Shared.Core`, `Shared.DTOs` |
| `Modules.<Name>.Infrastructure` | EF Core `DbContext`, design-time factory, migrations, seeders, services implementing Core interfaces | `.Core`, `Shared.Infrastructure` |
| `Modules.<Name>` | ASP.NET controllers, `ModuleExtensions.Add<Name>Module(config)` | `.Core`, `.Infrastructure` |

Test projects, where they exist, sit alongside as `Modules.<Name>.Core.Tests`
(Sales, Purchasing, Reporting) plus `Shared.Infrastructure.Tests`.

Controllers are `internal sealed` and picked up by a custom `InternalControllerFeatureProvider`
registered in `Shared.Infrastructure` — that is why they are not `public`.

---

## How modules are wired at startup

`src/server/API/Startup.cs` is the whole composition root:

```csharp
services
    .AddSerialization(_config)
    .AddSharedInfrastructure(_config)   // middleware, Swagger, JWT, CORS, health, Hangfire, cache
    .AddIdentityModule(_config)
    .AddSharedApplication(_config)
    .AddOrganizationsModule(_config)
    .AddCatalogModule(_config)
    .AddPeopleModule(_config)
    .AddSalesModule(_config)
    .AddInventoryModule(_config)
    .AddPurchasingModule(_config)
    .AddReportingModule(_config);
```

Order matters: Identity registers ASP.NET Identity before the shared application layer resolves it,
and Organizations registers `IStoreService` before Catalog/Sales consume it.

`app.UseSharedInfrastructure()` in `Configure` does the rest: it runs `Initialize()` (migrations +
seeding, both config-gated), then builds the middleware pipeline. Each module's `ModuleExtensions`
adds its own controllers to the `IMvcBuilder` and registers its own services — **adding a module is
a one-line change in `Startup.cs`.**

---

## Cross-module communication rules

These are the rules that keep the monolith modular. Breaking them is the main way this codebase
rots.

1. **No project references between modules.** `Modules.Sales.Core` must not reference
   `Modules.Catalog.Core`.
2. **No writing to another module's tables.** Each module owns its schema.
3. **Cross-cutting reads go through interfaces in `Shared.Core`**, implemented in the owning
   module's Infrastructure project. Current integration interfaces:

   | Interface | Implemented by | Used by |
   |---|---|---|
   | `IStoreService` | Organizations | Sales, Purchasing, Catalog |
   | `IProductService` | Catalog | Sales, Purchasing |
   | `IStoreProductService` | Catalog | Purchasing (replenishment) |
   | `IStockService` | Inventory | Sales (checkout/refund), Purchasing (receiving) |
   | `ICustomerService` | People | Sales |
   | `ICurrentUser`, `ITenantContext` | Shared.Infrastructure | Everywhere |

4. **Notifications, not calls, for reactions.** Domain/integration events are MediatR
   `INotification`s handled by `INotificationHandler<T>` in the reacting module.
5. **Shared DTOs live in `Shared.DTOs`.** That is the only type-sharing channel across boundaries.

---

## The feature pattern (CQRS + MediatR)

Inside `Modules.<Name>.Core`:

```
Features/<Entity>/
  Commands/                 IRequest<Result<T>> records + IRequestHandler
  Commands/Validators/      FluentValidation AbstractValidator<TCommand>
  Events/                   INotification + INotificationHandler
  Queries/                  IRequest<T> + IRequestHandler
  Queries/Validators/
```

Controllers do essentially nothing but `Mediator.Send(command)`. Handlers return
`Result<T>` / `PaginatedResult<T>` wrappers from `Shared.Core.Wrapper`.

MediatR pipeline behaviours registered in `Shared.Infrastructure`:

- **Validation** — FluentValidation runs before the handler; failures become HTTP 400.
- **Caching** (`CachingBehavior`) — caches `GetById`-style query responses. Cache keys are
  **partitioned by the caller's store**, so a store-scoped user cannot read another store's entity
  out of the cache.

Anything that runs on a schedule goes through `IJobService` (Hangfire), not `Task.Run`.

---

## Request pipeline

Order from `Shared.Infrastructure/Extensions/ApplicationBuilderExtensions.cs`:

```
Initialize()                     migrations + seeding (gated by PersistenceSettings flags)
  ↓
GlobalExceptionHandler           maps CustomException → problem responses
  ↓
UseRouting
  ↓
StaticFiles /files               product images
StaticFiles /pos                 POS PWA (only if the PosClient folder exists)
  ↓
CORS "CorsPolicy"                single origin from CorsSettings.Url
  ↓
Authentication (JWT)             validates issuer, audience, lifetime, signing key
Authorization                    PermissionPolicyProvider + PermissionAuthorizationHandler
  ↓
Hangfire dashboard /jobs         ⚠ no auth filter configured
  ↓
Endpoints                        controllers, /health/live, /health/ready
  ↓
Swagger + Swagger UI /swagger    v1 and v2 documents
```

Routing is configured with `LowercaseUrls = true`, so every path is lowercase regardless of how the
controller is named. API versioning uses `api/v{version:apiVersion}/...` with v1 assumed when
unspecified.

---

## Multi-tenancy: organizations, stores, terminals

The single most important concept in this fork.

```
Organization            the owning company — franchisor or franchisee; carries RoyaltyRatePercent
   └── Store            a physical site (the "node") — address, IsDefault flag, settings
          └── Terminal  a registered till/device — holds a hashed device key
```

### How scoping is enforced

1. **Claims.** `TokenService` stamps `storeId` and `orgId` claims on the JWT from
   `FluentUser.StoreId` / `FluentUser.OrganizationId`. A null `StoreId` means head office.
2. **`ITenantContext`** (`Shared.Infrastructure`) reads those claims off the current request.
3. **Global query filters.** `ModuleDbContext.OnModelCreating` applies a filter to *every* entity
   implementing `IMustHaveStore`, and auto-stamps the tenant's store on insert. Isolation is a
   property of the model, not a discipline each query has to remember.
4. **Reporting scoping** additionally reads `OrganizationId`: store staff see their store,
   franchisee managers see their organization, the franchisor sees everything.

### What is store-scoped vs. organization-scoped

| Store-scoped (`IMustHaveStore`) | Centrally owned (org-level master data) |
|---|---|
| `Stock`, `StockTransaction` (unique per store + product) | `Product`, `Brand`, `Category`, `VatRate` |
| `Order`, `Transaction` (payment), `Cart` | `Customer`, `Supplier` |
| `TillSession`, cash movements | |
| `PurchaseOrder` | |
| `StoreProduct` (the per-store overlay) | |

`StoreProduct` is the overlay that makes "add a product once, push it to every store" work: a store
inherits the central price unless a `StoreProduct` row overrides it, and the row also carries the
ranging flag, reorder point/quantity, and preferred supplier. Checkout resolves the effective price
through `IProductService.GetDetailsAsync(productId, storeId)`.

**Head-office behaviour:** a user with no `storeId` claim is unscoped for reads, and transacts
against the default store when a command does not name one. That is a deliberate convenience and a
sharp edge — see [known debt](#known-architectural-debt).

---

## Persistence: one database, nine contexts

One PostgreSQL database, one schema per module, one `DbContext` per module — all deriving from
`ModuleDbContext`, which is where auditing, event publishing, tenancy filters, and sync stamping
live.

| Context | Project | Schema owns |
|---|---|---|
| `ApplicationDbContext` | `Shared/Shared.Infrastructure` | Event logs, extended attributes, entity references |
| `IdentityDbContext` | `Modules/Identity/…Infrastructure` | Users, roles, claims, tokens |
| `OrganizationDbContext` | `Modules/Organizations/…Infrastructure` | Organizations, stores, terminals |
| `CatalogDbContext` | `Modules/Catalog/…Infrastructure` | Products, brands, categories, VAT rates, store products |
| `PeopleDbContext` | `Modules/People/…Infrastructure` | Customers, carts, cart items |
| `SalesDbContext` | `Modules/Sales/…Infrastructure` | Orders, order lines, transactions, till sessions |
| `InventoryDbContext` | `Modules/Inventory/…Infrastructure` | Stock, stock transactions |
| `PurchasingDbContext` | `Modules/Purchasing/…Infrastructure` | Suppliers, purchase orders, lines |
| `ReportingDbContext` | `Modules/Reporting/…Infrastructure` | `DailyStoreSales` read model |

Every context has an `IDesignTimeDbContextFactory`, so `dotnet ef` works without the host.

`SaveChangesAsync` on `ModuleDbContext` does three things beyond persisting: it writes audit fields
(`AuditableEntity`), stamps `ISyncTracked.LastModifiedOn` for the sync feed, and publishes queued
domain events via MediatR.

### Migrations

Applied automatically when `PersistenceSettings.MigrateOnStartup` is true. Manually:

```powershell
./migrate-database.ps1                    # all nine contexts, in dependency order
```

```bash
# Add a migration for one module
cd src/server/Modules/<Name>/Modules.<Name>.Infrastructure
dotnet ef migrations add <Name> --context <Name>DbContext --startup-project ../../../API
```

Migration history was squashed to a single `Initial` migration per context in August 2026 — there is
no production data and no upgrade path from pre-Phase-1 databases. A new environment bootstraps from
empty.

---

## Eventing and the reporting read model

Domain events are MediatR notifications published from inside `SaveChangesAsync`. Every one is also
persisted to the `EventLogs` table with user attribution (`Shared.Infrastructure/EventLogging/`) —
that is an **audit log, not a transactional outbox**. Nothing dispatches events beyond the process.

Two integration events (declared in `Shared.Core` so any module may handle them) drive reporting:

```
checkout / pos checkout / refund
        │
        ├─ OrderRegistered ─┐
        └─ OrderRefunded  ──┴─▶ Reporting handler ─▶ DailyStoreSales
                                                     (one row per store per day:
                                                      orders, gross, tax, refunds, net,
                                                      org snapshot, royalty accrual)
```

Royalty is computed continuously as `net × organization.RoyaltyRatePercent`, snapshotted at
projection time so a later rate change does not silently rewrite history. Projection failures are
logged and swallowed — a reporting bug must never fail a sale.

**Adding the transactional outbox is the highest-value next piece of work.** In-process events are
adequate for a single deployment, but store sync, webhooks, and partner APIs all need durable
delivery. `SaveChangeWithPublishEventsAsync` is the exact hook to write outbox rows in the same
transaction.

---

## The store-node sync protocol

Two endpoints make a till a node rather than a thin client.

**Pull — `GET api/v1/catalog/sync?since=<cursor>`**
Returns changed products, store overlays, and VAT rates, plus a `serverTime` the client persists as
its next cursor. Cursors are pure server clock, so device clock skew is irrelevant. Change detection
rides on `ISyncTracked.LastModifiedOn`, stamped centrally in `ModuleDbContext`.

**Push — `POST api/v1/sales/orders/pos`**
Takes the complete client-owned sale document. The device-generated `clientSaleId` **is** the order
id, so a replayed sale returns the existing order instead of double-charging. Anonymous sales are
attributed to a seeded walk-in customer.

That idempotency is what makes the durable client outbox safe: queue offline, replay blindly, never
double-charge.

The chosen model is **offline-first client against the cloud API**, not an edge server per store.
Stock at the node is a cached hint; the cloud stays authoritative and decrements on sync.
Negative-stock tolerance is normal in convenience retail. The protocol was designed so an edge-server
model stays possible later.

---

## Background jobs

Hangfire, backed by the same PostgreSQL database, dashboard at `/jobs`.

| Job | Trigger | What it does |
|---|---|---|
| Auto-replenishment | Hourly recurring, plus `POST api/v1/purchasing/replenishment/run` | Scans ranged `StoreProduct` rows with a reorder point, compares live per-store stock, creates **draft** POs grouped by store + preferred supplier, skips products already on open POs (idempotent) |

Schedule work through `IJobService`, never `Task.Run`.

---

## Caching

`CacheSettings.UseRedis` switches between `AddDistributedMemoryCache` (default, single-process) and
Redis at `CacheSettings.RedisConnectionString`. Compose enables Redis. `SlidingExpiration` is in
minutes. Cache keys used by `CachingBehavior` include the caller's store, so scoped users cannot
read across the tenancy boundary via the cache.

---

## Configuration reference

All of `appsettings.json` can be overridden by environment variables using `__` as the separator
(`PersistenceSettings__ConnectionStrings__postgres`).

| Section | Key | Default | Notes |
|---|---|---|---|
| `PersistenceSettings` | `UsePostgres` / `UseMsSql` | `true` / `false` | Exactly one |
| | `MigrateOnStartup` | `true` | **Set false with >1 replica**; run migrations as a release step |
| | `SeedOnStartup` | `true` | Set false in production |
| | `ConnectionStrings.postgres` | localhost | Override via env var |
| `JwtSettings` | `Key` | sample value | **Must be replaced.** 32+ chars |
| | `Issuer` / `Audience` | `FluentPOS` / `FluentPOS.Client` | Both validated |
| | `RequireHttpsMetadata` | `false` in the file, `true` in code default | `true` in production |
| | `TokenExpirationInMinutes` | `60` | |
| | `RefreshTokenExpirationInDays` | `7` | |
| `CacheSettings` | `UseRedis` | `false` | |
| | `RedisConnectionString` | `localhost:6379` | |
| | `SlidingExpiration` | `2` | Minutes |
| `CorsSettings` | `Url` | `http://localhost:4200` | Single allowed origin |
| `ApplicationSettings` | `ApiUrl` | `https://localhost:5001/` | Used to build absolute file URLs |
| `MailSettings` / `SmsSettings` | — | sample/ethereal | Replace before enabling verification |
| `SerializationSettings` | `UseSystemTextJson` | `true` | Newtonsoft is the alternative |

---

## Known architectural debt

Carried forward deliberately; each is a candidate for the next phase.

1. **No transactional outbox.** Events are in-process only. Blocks webhooks, partner APIs, and
   durable store sync. Highest priority.
2. **Hangfire dashboard `/jobs` is unauthenticated.** Fine on localhost, unacceptable when exposed —
   see [deployment.md](deployment.md).
3. **Accounting module is an empty shell** in the solution but not in the host.
4. **Angular 12 client is EOL** and covers only the pre-multi-store feature set.
5. **Head-office users transact against the default store** when a command omits `storeId` — quiet
   and occasionally wrong.
6. **Store/organization assignment for users is seed/DB-only** — there is no admin API to move a user
   between stores.
7. **No catalog tombstones.** Deleted products linger in device caches until a full resync.
8. **Queued sales rejected as permanently invalid are dropped client-side**; a server-side
   dead-letter would be better.
9. **Refunds are full-order only.** No partial refunds, no per-terminal receipt numbering, no
   promotions or loyalty.
10. **No margin reporting** — order lines do not snapshot cost.

See [EPOS_TRANSFORMATION_PLAN.md](../EPOS_TRANSFORMATION_PLAN.md) for the sequenced backlog.
