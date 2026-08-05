# FluentPOS → Multi-Site Cloud EPOS: Assessment & Transformation Plan

**Goal:** Evolve FluentPOS into a cloud-native, multi-site, API-driven EPOS where each store is a
node in a network governed by a Central Cloud Core (master data, centralized pricing, chain-wide BI,
eventually franchise features).

**Date:** 2026-07-07 · **Baseline:** master @ c6f00026 (post .NET 10 migration)

---

## 1. Assessment of the current codebase

### 1.1 What we have (the good bones)

FluentPOS is architecturally a strong starting point for an API-driven system:

| Area | Current state |
|---|---|
| Architecture | Modular monolith; modules (Catalog, Identity, Inventory, People, Sales) isolated as Core/Infrastructure/API projects; cross-module access only via `Shared.Core` interfaces |
| Stack | .NET 10, EF Core 10, PostgreSQL (or MSSQL), MediatR 14 CQRS, FluentValidation, AutoMapper, Hangfire, Serilog |
| API surface | REST controllers per module, API versioning (v1/v2), Swagger/OpenAPI, JWT auth |
| Authorization | Fine-grained claim-based permission system (`Shared.Core/Constants/Permissions.cs`, `PermissionPolicyProvider` + `PermissionAuthorizationHandler`) with seeded roles (SuperAdmin, Admin, Manager, Accountant, Cashier, Staff) |
| Persistence | Schema-per-module in one database via `ModuleDbContext`; migrations + seeding run on startup |
| Auditing | Every domain event is persisted to an `EventLogs` table with user attribution (`Shared.Infrastructure/EventLogging/EventLogger.cs`) |

The "API-driven" pillar is essentially already true. The other two pillars — **cloud-native** and
**multi-site** — do not exist yet.

### 1.2 What is missing (gap analysis)

**Multi-site: nothing exists.** A thorough search found zero occurrences of Tenant/Store/Branch/
Location/Warehouse/Organization in any entity, DbContext, token claim, or query. Specifically:

- `Stock` (`Modules/Inventory/.../Entities/Stock.cs`) is one row per product **globally** — no store dimension.
- JWT tokens (`TokenService.cs`) carry roles and permissions but no store or organization scoping.
- All queries are unscoped; there is no global query filter or tenant middleware.

**Eventing is in-process only.** Domain events are MediatR notifications published inside
`SaveChangesAsync` (`ModuleDbContextExtensions.SaveChangeWithPublishEventsAsync`). The `EventLogs`
table is an audit log, **not** a transactional outbox — nothing dispatches events beyond the
process. Cross-module coordination at checkout is done by direct synchronous service calls
(`SaleCommandHandler` → `IStockService.RecordTransaction`). Fine inside one process; a blocker for
store-node sync.

**Retail functionality is a demo, not an EPOS.** Missing entirely:

- **Purchasing:** no Supplier, PurchaseOrder, or goods-received flow. `TransactionType` enum has a
  `Purchase` value but no workflow behind it. `Product.AlertQuantity`/`IsAlert` are stored but
  consumed by no logic — no reorder-point automation.
- **Till operations:** no register/till/cash-drawer session, no Z-reports, no cash reconciliation.
- **Payments:** the `Transaction` (payment) entity exists but is **never written** — checkout
  creates an `Order` and stops. No receipts, no refunds/voids/returns.
- **Pricing/tax:** single `Price` + `Cost` per product; no price lists, no per-store pricing, no
  margin logic. Tax is a free-text `TaxMethod` string + flat decimal — no UK VAT rate model.
- **Barcodes:** `Product.BarcodeSymbology` names a symbology; there is **no field storing the actual
  barcode/EAN value**. Wholesaler EAN mapping is impossible today.
- **Compliance:** no age-restricted-product flag, no Challenge 25 prompt hook, no DOB capture.
- **Reporting/BI:** none — only paginated list queries. Accounting module is an empty shell
  (csproj files, zero source).
- **Promotions/loyalty:** none (a `Discount` field exists on orders but nothing ever sets it).

**Cloud-native ops: nothing exists.** No Dockerfile/compose/k8s, no health checks, in-memory cache
only (no Redis), JWT configured with `ValidateIssuer=false`, `ValidateAudience=false`,
`RequireHttpsMetadata=false`. Migrations and seeding run on startup (unsafe with >1 replica).

**Client is end-of-life and cloud-only.** Angular 12 (EOL, TSLint/Protractor, needs
`--openssl-legacy-provider`). No PWA/service worker/IndexedDB — zero offline capability. Cart state
is authoritative on the **server** (People module Carts API), which means the till stops working the
moment connectivity drops. For a convenience store, that is unacceptable: offline-capable selling is
the defining requirement of a store "node."

### 1.3 Honest verdict

FluentPOS gives you a clean, well-patterned skeleton — the module pattern, CQRS plumbing, permission
system, and audit trail are genuinely reusable. But functionally it is a single-store demo. The
three hardest parts of your target system — **tenancy, purchasing/replenishment, and offline store
nodes** — must all be built new. Treat this as "we have a good framework and coding conventions,"
not "we have 60% of an EPOS." Expect the transformation below to be 12–24 months of serious
engineering for a small team. (If the business goal were only *running stores*, buying Epos Now /
Lightspeed is rationally cheaper; building is defensible if the EPOS platform itself is intended to
be an asset — e.g. the thing you license to franchisees.)

---

## 2. Target architecture

```
                        ┌──────────────────────────────────────────────┐
                        │              CENTRAL CLOUD CORE              │
                        │  (single multi-tenant deployment, Postgres)  │
                        │                                              │
                        │  Organization module   Catalog (master data) │
                        │  Purchasing module     Pricing (per-store    │
                        │  Reporting read models   overrides)          │
                        │  Identity (org/store-scoped tokens)          │
                        │  Outbox → event bus → sync + BI + webhooks   │
                        └───────┬───────────────┬───────────────┬──────┘
                                │ HTTPS + sync protocol (idempotent,
                                │ client-generated IDs, sequence cursors)
                    ┌───────────┴───┐   ┌───────┴───────┐   ┌───┴───────────┐
                    │  STORE NODE 1 │   │  STORE NODE 2 │   │  STORE NODE N │
                    │  offline-first│   │               │   │  (franchisee: │
                    │  POS client(s)│   │               │   │  scoped view) │
                    │  local product│   │               │   │               │
                    │  cache + sale │   │               │   │               │
                    │  outbox queue │   │               │   │               │
                    └───────────────┘   └───────────────┘   └───────────────┘
```

### 2.1 Tenancy model

Three-level hierarchy, added as a new **Organization module** (follows the existing module pattern):

- **Organization** — the owning company (later: one per franchisee, plus the franchisor org).
- **Store** — a physical site (the "node"). Holds address, licence metadata, settings.
- **Terminal** — a registered till/device within a store (needed for receipt numbering, cash
  sessions, and device auth).

Plumbing (this is the single most invasive change, so do it early):

1. New interfaces in `Shared.Core`: `IMustHaveOrganization { Guid OrganizationId }`,
   `IMustHaveStore { Guid StoreId }`.
2. Extend `ICurrentUser` to expose `OrganizationId` and permitted `StoreIds` from token claims;
   `TokenService` adds `org_id` / `store_id` claims at issuance.
3. `ModuleDbContext.OnModelCreating` applies **global query filters** for entities implementing the
   tenancy interfaces, driven by an injected `ITenantContext`. This makes store isolation the
   default, not a per-query discipline — critical for the franchise requirement ("franchisees see
   only their stores; franchisor sees macro view").
4. Store-scoped entities: `Stock` and `StockTransaction` (composite key StoreId+ProductId), Orders,
   Carts, till sessions, per-store price overrides. Org-scoped (master data): Products, Brands,
   Categories, Customers, Suppliers.

### 2.2 Master data management (add once centrally, push everywhere)

- Catalog stays **centrally owned** (org-level). Add a `StoreProduct` overlay entity for per-store
  overrides: sell price (optional), active/ranged flag, reorder point, reorder quantity, preferred
  supplier. Default behaviour: store inherits central price — exactly your "add a product once,
  push to all stores" requirement.
- Product model upgrades: **`Barcode` value field** (support multiple EANs per product — case vs.
  single unit), `VatRateId` referencing a proper UK VAT rate table (0% / 5% / 20%) instead of the
  free-text `TaxMethod`, `IsAgeRestricted` + `MinimumAge` for Challenge 25, cost-price history.

### 2.3 Eventing: from in-process to outbox

Keep MediatR for in-process handlers, and add a real **transactional outbox**:

1. New `OutboxMessage` table written in the same transaction as the aggregate (the hook already
   exists — `SaveChangeWithPublishEventsAsync` is the exact place).
2. A Hangfire recurring job (already have Hangfire) dispatches pending messages. Start with
   Hangfire-as-bus; introduce RabbitMQ/Azure Service Bus only when consumers multiply.
3. Integration events (versioned contracts in `Shared.DTOs`) become the backbone for: store sync,
   reporting projections, webhooks (delivery platforms), and later royalty calculation triggers.

### 2.4 Store nodes: offline-first client, not edge servers (initially)

Two candidate patterns:

- **A. Offline-first POS client** against the cloud API: PWA/desktop app with a local product cache
  (IndexedDB/SQLite), local cart, and a durable outbound queue of completed sales that replays when
  connectivity returns. Cloud remains the single source of truth.
- **B. Edge server per store**: a local instance of the API + DB with bidirectional sync.
  Maximum autonomy, but you inherit distributed-systems problems (conflict resolution, schema
  versioning across a fleet, remote ops) on day one.

**Recommendation: A first, designed so B stays possible.** The enablers are protocol-level and
cheap to do now:

- **Client-generated sale IDs (UUIDs) + idempotency keys** on `RegisterSaleCommand`, so a queued
  sale can be safely replayed. (Today `RegisterSaleCommand` takes a server-side `CartId` — this
  inverts: the client submits the complete sale document.)
- **Move cart ownership to the client.** The server-side Cart (People module) becomes unnecessary
  for POS flow; checkout accepts the full basket. This is also what makes offline selling work.
- **Catalog sync feed:** a cursor/sequence-numbered "changes since X" endpoint per store so nodes
  pull incremental product/price updates (and later, push-notify via SignalR/webhook).
- Stock at the node is a *cached hint*, authoritative in the cloud; sales decrement centrally on
  sync. Negative-stock tolerance is normal in convenience retail.

### 2.5 Cloud-native operations

- Dockerfile + docker-compose (API, Postgres, Redis); health endpoints
  (`AddHealthChecks` + DB/Hangfire probes); readiness/liveness split.
- Move `Migrate()` + seeding out of `Startup.Initialize()` into a dedicated migration job/CLI step
  (safe for multiple replicas).
- Redis for distributed cache (replace `AddDistributedMemoryCache`) and Hangfire coordination.
- JWT hardening: `ValidateIssuer/Audience = true`, HTTPS metadata on, key from secrets store.
- OpenTelemetry traces/metrics/logs; per-store dashboards fall out of this naturally.

---

## 3. Phased roadmap

### Phase 0 — Foundation hardening (weeks, not months) — ✅ DONE (2026-07-08)
Prepares the ground; no functional change visible to users.
1. ✅ Dockerfile + compose (API/Postgres/Redis, `.env.example`); `/health/live` + `/health/ready`;
   migrations/seeding gated behind `PersistenceSettings.MigrateOnStartup`/`SeedOnStartup`; Redis via
   `CacheSettings.UseRedis` (memory cache fallback).
2. ✅ JWT hardening: issuer/audience validation on, `RequireHttpsMetadata` config-driven, key
   overridable via env var (`JwtSettings__Key`); refresh flow fixed to accept expired access tokens.
3. ✅ Sale flow gaps: checkout persists a `Transaction` payment record and marks orders paid; order
   header totals now computed; `Product.Barcode` (EAN value, unique) added; `VatRate` table seeded
   with UK rates (Zero/Reduced/Standard) + `GET api/v1/catalog/vatrates`; `Product.VatRateId` drives
   `Tax` (old `Tax`/`TaxMethod` kept for client compatibility, retire in Phase 1).
4. ✅ CI runs build + tests on .NET 10; new `Modules.Sales.Core.Tests` unit-test project.
5. ✅ Latent bugs fixed along the way: `DateTime.Now` writes rejected by Postgres `timestamptz`
   (Order/Cart/Stock/StockTransaction/EntityReference → UtcNow), untranslatable
   OrderBy-after-ProjectTo queries in product/sales list endpoints, barcode uniqueness checked
   against the symbology name, permission claims filtered by hardcoded `LOCAL AUTHORITY` issuer.

### Phase 1 — Multi-store core (the big one) — ✅ DONE (2026-08-05)
1. ✅ Organizations module (`Modules/Organizations/*`): Organization, Store (with `IsDefault`),
   Terminal entities; Store CRUD + Terminal register/list + Org list APIs under
   `api/v1/organization/*`; seeded org + two stores (fixed GUIDs in
   `Shared.Core/Constants/OrganizationConstants.cs`) + one till each; `IStoreService` integration
   interface (exists/default-store).
2. ✅ Tenancy plumbing: `IMustHaveStore` marker, `ITenantContext` (reads `storeId` JWT claim),
   `ModuleDbContext` applies a global query filter to every `IMustHaveStore` entity and
   auto-stamps the tenant's store on insert. Head-office users (no store claim) are unscoped.
3. ✅ Store dimension: `Stock`/`StockTransaction` (unique per store+product), `Order`,
   `Transaction`, `Cart` all store-stamped; carts resolve store from command → token → default
   store; sales inherit the cart's store; `StoreProduct` overlay (price override, ranging flag,
   reorder point/qty) with upsert/list/remove APIs; checkout uses the store-effective price via
   `IProductService.GetDetailsAsync(productId, storeId)`.
4. ✅ Store-scoped identity: `FluentUser.StoreId` (null = HQ), `storeId` claim in tokens, staff
   seeded to Store One (backfilled on existing DBs), Staff role granted the POS permission set.
   Existing store-scoped rows backfilled to the default store in migrations.
5. ✅ Exit criteria verified end-to-end: two seeded stores; central product sold in both with
   Store Two price override honored at checkout (99.99 vs 200); independent stock rows per store;
   staff token scoped to Store One; staff blocked from other-store carts (403), other-store orders
   invisible (404/filtered lists) — enforced at the EF query-filter level, not per query.

   Known Phase 1 limitations (carry into Phase 2): `GetById` response caching is not store-aware
   (a scoped user who knows a foreign entity's GUID could read it from cache — key caches by
   store or bypass cache for store-scoped entities); HQ users transact against the default store
   when no store is specified; store assignment for users is seed/DB-only (no admin API yet).

   **Fresh-database reset (2026-08-05):** since no production data exists, all historical
   migrations were squashed into a single `Initial` migration per module context (including
   Identity and the shared Application context, which gained a design-time factory). Legacy
   compatibility shims were removed: no store backfills, and `Product.Tax`/`TaxMethod`/
   `IsAlert`/`AlertQuantity` are gone — `Product.VatRateId` is now required and the single
   source of truth for tax (DTOs expose a computed `Tax` percentage), reorder settings live
   solely on `StoreProduct`, and checkout computes line tax as `price × qty × rate%`. A new
   environment bootstraps from empty: run the API once and it migrates + seeds everything.

### Phase 2 — Retail operations (make it a real EPOS)
1. **Purchasing module** (new, follows module pattern): Supplier, PurchaseOrder + lines,
   GoodsReceipt; receiving increments stock via the existing `IStockService` path.
2. **Auto-replenishment:** Hangfire job scanning per-store stock vs. `StoreProduct.ReorderPoint`,
   generating **draft** POs grouped by preferred supplier (your "gin below 6 bottles" scenario).
3. **Till sessions:** open/close register, cash float, payout/pickup, X/Z reports, reconciliation.
4. Receipts (numbered per terminal), refunds/voids with reason codes, basic promotions
   (multibuy/percent-off) if needed for launch.
5. **Challenge 25:** age-restricted flag drives a mandatory verification prompt + audit record at
   the till (licensing inspections ask for this).

### Phase 3 — Store node resilience (offline-first client)
1. New POS front end (the Angular 12 app is EOL — POS screen is a rewrite regardless; keep the
   admin back-office on the old app until it is rewritten or replaced): PWA or lightweight desktop
   shell, IndexedDB product cache, client-owned cart, durable sale queue.
2. Sync protocol: idempotent sale submission (client UUIDs), catalog change feed with cursors,
   clock-skew-tolerant timestamps.
3. Terminal registration/device auth (long-lived device credential + short-lived operator PIN
   sign-in — cashiers don't type passwords).

### Phase 4 — Chain & franchise layer
1. Multi-organization: franchisee orgs, franchisor macro views, cross-org consolidated reporting.
2. Reporting module: event-projected read models (sales by store/hour/category, margin, shrinkage),
   replacing "query the transactional tables harder."
3. Royalty triggers: periodic jobs off sales integration events computing royalty per franchise
   agreement.
4. Wholesaler integrations (Booker/Bestway/Nisa): import price files/EAN catalogs mapping to
   Products via barcode; export POs. Design as adapters in the Purchasing module.
5. Public API + webhooks for delivery platforms (Snappy Shopper, Deliveroo) — the API-driven
   surface already exists; this adds outbound eventing and partner auth.

---

## 4. Key risks & decisions to make early

1. **Client rewrite scope** — Angular 12 is unmaintainable; decide the POS front-end stack in
   Phase 0 (Angular 20 rewrite vs. React vs. .NET MAUI/desktop). The offline-first requirement
   should drive this choice.
2. **Tenancy retrofit is invasive** — every store-scoped table changes shape. Do it before any
   feature work (Phase 1 before Phase 2), while the data model is still small and there is no
   production data to migrate.
3. **Don't skip the outbox** — building store sync on fire-and-forget in-process events will lose
   sales records. The outbox is the cheapest insurance in the plan.
4. **Payments hardware** — card terminal integration (e.g. Dojo, SumUp, Adyen) is out of scope of
   this codebase today but is on the critical path for a real store; start commercial conversations
   in parallel with Phase 1.
5. **Build-vs-buy checkpoint** — re-evaluate at the end of Phase 1. If the platform is not itself
   the business asset, a commercial EPOS plus this codebase's central-reporting layer may be the
   faster route to store #1.
