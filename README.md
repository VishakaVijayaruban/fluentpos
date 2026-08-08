<p align="center">
  <h3 align="center">FluentPOS — Multi-Site Cloud EPOS</h3>
  <p align="center">
    A modular-monolith point-of-sale and inventory platform built with ASP.NET Core 10 and PostgreSQL,
    with an offline-first till client.
  </p>
</p>

---

## What this is

This repository is a **working fork of [fluentpos/fluentpos](https://github.com/fluentpos/fluentpos)**
(archived upstream, ASP.NET Core 5 + Angular 12), being transformed into a cloud-native, multi-site,
API-driven EPOS in which **each store is a node in a network governed by a central cloud core**.

Four phases of that transformation are complete:

| Phase | Delivered | Status |
|---|---|---|
| **0 — Foundation hardening** | .NET 10, Docker + compose, health checks, JWT hardening, Redis, UK VAT rates, real payment records, product barcodes, CI | ✅ |
| **1 — Multi-store core** | Organization/Store/Terminal model, tenancy via EF global query filters, store-scoped stock and orders, `StoreProduct` price overlays, store-scoped tokens | ✅ |
| **2 — Retail operations** | Purchasing (suppliers, POs, goods-in), auto-replenishment, till sessions with X/Z reports and cash reconciliation, refunds, Challenge 25 | ✅ |
| **3 — Store-node resilience** | Offline-first POS PWA with IndexedDB catalog cache and durable sale outbox, incremental sync protocol, idempotent checkout, terminal device auth + operator PIN | ✅ |
| **4 — Chain & franchise layer** | Multi-organization tenancy with royalty rates, event-projected sales reporting, royalty accrual, wholesaler price-file import and PO export | ✅ (webhooks deferred) |
| **5 — Next** | Transactional outbox, integration tests, client replacement, card payments | 📋 See [the plan](EPOS_TRANSFORMATION_PLAN.md#5-where-we-are-and-what-is-next) |

The full assessment, target architecture, and sequenced backlog live in
**[EPOS_TRANSFORMATION_PLAN.md](EPOS_TRANSFORMATION_PLAN.md)** — read it to understand *why* the code
looks the way it does.

---

## Quick start

### Docker (fastest — needs only Docker)

```bash
git clone <this-repo> && cd fluentpos
cp .env.example .env
# Set JWT_KEY in .env to 32+ random characters — compose will not start without it
docker compose up --build
```

API at <http://localhost:5000> · Swagger at `/swagger` · POS till at `/pos`.

### Local (for development — needs .NET 10 SDK + PostgreSQL)

```bash
# 1. Point the API at your PostgreSQL
#    src/server/API/appsettings.json → PersistenceSettings.ConnectionStrings.postgres

# 2. Run it — the database is created, migrated and seeded automatically
dotnet run --project src/server/API
```

Then log in:

```bash
curl -s -X POST http://localhost:5000/api/v1/identity/tokens \
  -H "Content-Type: application/json" \
  -d '{"email":"superadmin@fluentpos.com","password":"123Pa$$word!"}'
```

Full walkthrough, including the Angular client: **[docs/getting-started.md](docs/getting-started.md)**.

---

## Documentation

**→ Start at [docs/README.md](docs/README.md)**

| Doc | Answers |
|---|---|
| [Getting Started](docs/getting-started.md) | Install, configure, run, log in, run both clients |
| [Architecture](docs/architecture.md) | Modules, tenancy, CQRS, eventing, sync protocol, config reference |
| [Users & Access](docs/users-and-access.md) | Roles, permissions, store scoping, creating users, till PINs, franchisee onboarding |
| [Seed Data](docs/seed-data.md) | What is in the database after a fresh boot; fixed GUIDs; resetting |
| [API Reference](docs/api-reference.md) | Every endpoint with its required permission |
| [Testing Guide](docs/testing-guide.md) | Automated tests plus ten end-to-end manual scenarios |
| [Build, Run & Deploy](docs/deployment.md) | Docker, compose, secrets, migrations as a release step, production checklist |
| [Troubleshooting](docs/troubleshooting.md) | Startup, auth, tenancy, migrations, client and PWA problems |

Also: [UBIQUITOUS_LANGUAGE.md](UBIQUITOUS_LANGUAGE.md) (domain vocabulary) ·
[CONTRIBUTING.md](CONTRIBUTING.md) · [CLAUDE.md](CLAUDE.md) / [AGENTS.md](AGENTS.md) (AI agent
conventions).

---

## Architecture at a glance

A **modular monolith**: one deployable API composed of isolated modules, each following Clean
Architecture. Modules never reference each other's projects or tables — they communicate through
interfaces in `Shared.Core` and MediatR notifications. One PostgreSQL database, one schema per module.

```
                    ┌──────────────────────────────────────────────┐
                    │              CENTRAL CLOUD CORE              │
                    │   Identity · Organizations · Catalog         │
                    │   People · Sales · Inventory                 │
                    │   Purchasing · Reporting                     │
                    │   Tenancy via EF global query filters        │
                    └───────┬───────────────┬───────────────┬──────┘
                            │  HTTPS · incremental catalog sync
                            │  · idempotent client-owned sales
                ┌───────────┴───┐   ┌───────┴───────┐   ┌───┴───────────┐
                │  STORE NODE 1 │   │  STORE NODE 2 │   │  STORE NODE N │
                │ offline-first │   │               │   │  (franchisee: │
                │ PWA till      │   │               │   │  scoped view) │
                │ IndexedDB     │   │               │   │               │
                │ sale outbox   │   │               │   │               │
                └───────────────┘   └───────────────┘   └───────────────┘
```

### Modules

| Module | Route prefix | Owns |
|---|---|---|
| Identity | `api/v1/identity/*` | Users, roles, permission claims, JWT, PIN/device sign-in |
| Organizations | `api/v1/organization/*` | Organizations, stores, terminals, franchisee onboarding |
| Catalog | `api/v1/catalog/*` | Products, brands, categories, VAT rates, store overlays, sync feed |
| People | `api/v1/people/*` | Customers, carts |
| Sales | `api/v1/sales/*` | Orders, payments, refunds, till sessions |
| Inventory | — | Per-store stock (consumed via `IStockService`) |
| Purchasing | `api/v1/purchasing/*` | Suppliers, purchase orders, replenishment, price files |
| Reporting | `api/v1/reporting/*` | Daily store sales read model, royalty accrual |
| Accounting | — | Empty shell — csproj files only, not wired into the host |

Deeper: [docs/architecture.md](docs/architecture.md).

### Key design decisions

- **Tenancy is enforced by the data model, not by discipline.** Every entity implementing
  `IMustHaveStore` gets an EF global query filter driven by the caller's `storeId` token claim, and is
  auto-stamped on insert. Store isolation is the default.
- **Central master data, per-store overrides.** Add a product once; every store inherits it. A
  `StoreProduct` overlay row optionally overrides sell price, ranging, reorder point and preferred
  supplier.
- **Checkout is idempotent.** The POS client owns the basket and submits a complete sale document
  whose device-generated UUID *is* the order id — so a sale queued offline can be replayed blindly
  without ever double-charging.
- **Offline-first client, not edge servers.** Each till caches the catalog and queues sales locally;
  the cloud stays the source of truth. The protocol was designed so an edge-server model remains
  possible later.

---

## Technology stack

| Layer | Choice |
|---|---|
| API | ASP.NET Core **10** WebAPI, API versioning (v1/v2), Swagger/OpenAPI |
| Patterns | Modular monolith, Clean Architecture per module, CQRS via **MediatR 14** |
| Data | **EF Core 10**, PostgreSQL (default) or MSSQL, schema per module |
| Validation / mapping | FluentValidation, AutoMapper |
| Auth | JWT bearer, claim-based fine-grained permissions, ASP.NET Identity |
| Jobs | Hangfire (Postgres-backed), dashboard at `/jobs` |
| Cache | In-memory, or Redis via `CacheSettings.UseRedis` |
| Logging | Serilog |
| Ops | Docker, docker-compose, `/health/live` + `/health/ready` |
| POS client | Dependency-free PWA (IndexedDB + service worker), served at `/pos` |
| Back office | Angular 12 Material — **legacy**, pre-multi-store features only |

---

## Repository layout

```
src/server/API/                    Host, Startup, appsettings, PosClient/ (the PWA)
src/server/Modules/<Name>/         Modules.<Name>.Core · .Infrastructure · (controllers)
src/server/Shared/                 Shared.Core · Shared.DTOs · Shared.Infrastructure
src/client/                        Angular 12 back-office app
docs/                              Documentation — start at docs/README.md
postman/                           API collection (predates the multi-store work)
docker-compose.yml                 API + PostgreSQL 16 + Redis 7
migrate-database.ps1               Applies migrations for all nine DbContexts
EPOS_TRANSFORMATION_PLAN.md        Assessment, roadmap, phase status, backlog
UBIQUITOUS_LANGUAGE.md             Domain vocabulary
```

---

## Common commands

```bash
# Server
dotnet build src/server/FluentPOS.sln          # 0 errors (~85 StyleCop warnings are known/benign)
dotnet test  src/server/FluentPOS.sln          # 33 unit tests
dotnet run   --project src/server/API
dotnet watch run --project src/server/API      # hot reload

# Migrations (only needed when MigrateOnStartup is off)
./migrate-database.ps1

# Client — needs Node 14/16; Angular 12 will not build on modern Node
cd src/client && npm install && npm run start  # http://localhost:4200

# Docker
docker compose up --build -d
docker compose logs -f api
docker compose down -v                         # also wipes the database
```

---

## Default credentials

Seeded on first run. **Change these before exposing the API to anyone.**

| Email | Password | Role | Scope |
|---|---|---|---|
| `superadmin@fluentpos.com` | `123Pa$$word!` | SuperAdmin | Head office — every permission |
| `staff@fluentpos.com` | `123Pa$$word!` | Staff | Store One |
| `franchisee@fluentpos.com` | `123Pa$$word!` | Manager | Northern Franchise Ltd |

Note that of the six seeded roles, **only SuperAdmin, Staff and Manager receive any permissions** —
Admin, Accountant and Cashier are name-only placeholders. Details:
[docs/users-and-access.md](docs/users-and-access.md).

---

## Project status

**This is pre-production.** It runs, it is coherent, and the retail flows work end to end — but read
[deployment.md § what is still missing](docs/deployment.md#what-is-still-missing-for-production)
before deploying anything. The headlines:

- **No transactional outbox** — events are in-process only. Blocks webhooks and durable store sync.
  Highest-priority next piece of work.
- **No integration tests** — 33 unit tests cover domain logic; nothing covers HTTP + EF query filters
  + permissions together.
- **The Hangfire dashboard at `/jobs` is unauthenticated.**
- **No card-payment integration.**
- **The Angular client is EOL** and has no UI for stores, purchasing, till sessions, refunds or
  reporting — those are API-only today.

---

## Contributing

- Branch naming: `fluentpos-<issueId>`, targeting `master`.
- Commit messages: present tense with a scope prefix — `API: add product search endpoint`,
  `NG: fix cart total calculation`, `docs: ...`.
- C#: StyleCop via `src/server/stylecop.json` and `src/server/fluentpos.ruleset`. 4-space indent.
- Run `dotnet build` and `dotnet test` before opening a PR.
- Respect the module boundaries — no cross-module project references, no writing to another module's
  tables. See [architecture.md § cross-module rules](docs/architecture.md#cross-module-communication-rules).
- Add tests for domain logic changes; see [testing-guide.md](docs/testing-guide.md#writing-new-tests).

More: [CONTRIBUTING.md](CONTRIBUTING.md) · [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

---

## Credits

Original FluentPOS by Mukesh Murugan ([@iammukeshm](https://github.com/iammukeshm/)),
Chhin Sras ([@chhinsras](https://github.com/chhinsras)), and
Nikolay Chebotov ([@unchase](https://github.com/unchase)). Upstream is archived; this fork continues
from it.

## License

[MIT](LICENSE).
