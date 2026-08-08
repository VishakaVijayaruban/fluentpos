# Seed Data

Exactly what lands in the database on a fresh boot, the fixed GUIDs you can hardcode in scripts, and
how to reset or replace it.

- [How seeding works](#how-seeding-works)
- [What gets seeded](#what-gets-seeded)
- [Fixed GUIDs](#fixed-guids)
- [The seeded scenario](#the-seeded-scenario)
- [What is deliberately *not* seeded](#what-is-deliberately-not-seeded)
- [Resetting the database](#resetting-the-database)
- [Turning seeding off](#turning-seeding-off)
- [Adding your own seed data](#adding-your-own-seed-data)
- [Loading a realistic catalog](#loading-a-realistic-catalog)

---

## How seeding works

On startup `app.UseSharedInfrastructure()` calls `Initialize()`, which:

1. If `PersistenceSettings.MigrateOnStartup` — runs every registered `IDatabaseMigrator`, creating the
   database and applying migrations for all nine DbContexts.
2. If `PersistenceSettings.SeedOnStartup` — runs every registered `IDatabaseSeeder`.

Both default to `true` in `appsettings.json`.

Seeders are **idempotent by existence check** — each one asks "is this table empty / does this row
exist?" before inserting. Restarting the API does not duplicate data, but it also does **not** update
data whose shape changed: if a table already has rows, the seeder skips it entirely.

Five seeders run:

| Seeder | Project |
|---|---|
| `IdentityDbSeeder` | `Modules/Identity/Modules.Identity.Infrastructure/Persistence/` |
| `OrganizationDbSeeder` | `Modules/Organizations/…/Persistence/` |
| `CatalogDbSeeder` | `Modules/Catalog/…/Persistence/` |
| `PeopleDbSeeder` | `Modules/People/…/Persistence/` |
| `PurchasingDbSeeder` | `Modules/Purchasing/…/Persistence/` |

Sales, Inventory and Reporting seed nothing — their data is produced by transacting.

---

## What gets seeded

| Data | Count | Source | Notes |
|---|---|---|---|
| Roles | 6 | `IdentityDbSeeder` | SuperAdmin, Admin, Manager, Accountant, Cashier, Staff |
| Permission claims | all → SuperAdmin; a POS subset → Staff; a reporting subset → Manager | `IdentityDbSeeder` | Admin/Accountant/Cashier get **none** |
| Users | 3 | `IdentityDbSeeder` | See [users-and-access.md](users-and-access.md#seeded-users) |
| Organizations | 2 | `OrganizationDbSeeder` | Franchisor + sample franchisee |
| Stores | 2 | `OrganizationDbSeeder` | Store One (default), Store Two |
| Terminals | 2 | `OrganizationDbSeeder` | "Till 1" in each store |
| VAT rates | 3 | `CatalogDbSeeder` | UK Zero 0% / Reduced 5% / Standard 20% |
| Brands | 15 | `SeedData/brands.json` | |
| Categories | 10 | `SeedData/categories.json` | |
| Products | 42 | `SeedData/products.json` | Default to the Standard VAT rate |
| Customers | 20 + walk-in | `SeedData/customers.json` + code | Walk-in has a fixed GUID |
| Suppliers | 1 | `PurchasingDbSeeder` | "Booker Wholesale" |

JSON seed files live in each module's `Infrastructure/Persistence/SeedData/` folder and are copied to
the build output.

---

## Fixed GUIDs

Stable across every environment, so you can paste them straight into scripts and tests.
Source: `src/server/Shared/Shared.Core/Constants/OrganizationConstants.cs` and the module seeders.

### Organizations

| Name | Id | Royalty |
|---|---|---|
| FluentPOS Retail (franchisor) | `7a000000-0000-4000-8000-000000000001` | 0% |
| Northern Franchise Ltd (franchisee) | `7a000000-0000-4000-8000-000000000002` | 5% |

### Stores

| Name | Id | Organization | City | Default |
|---|---|---|---|---|
| Store One | `51000000-0000-4000-8000-000000000001` | FluentPOS Retail | Manchester | ✅ |
| Store Two | `51000000-0000-4000-8000-000000000002` | Northern Franchise Ltd | Leeds | |

### Terminals

| Name | Id | Store |
|---|---|---|
| Till 1 | `71000000-0000-4000-8000-000000000001` | Store One |
| Till 1 | `71000000-0000-4000-8000-000000000002` | Store Two |

### VAT rates

| Name | Id | Rate |
|---|---|---|
| Zero | `6f3a1a2b-0000-4000-8000-000000000001` | 0% |
| Reduced | `6f3a1a2b-0000-4000-8000-000000000002` | 5% |
| Standard | `6f3a1a2b-0000-4000-8000-000000000003` | 20% |

### Other

| Thing | Id |
|---|---|
| Walk-in customer (anonymous till sales) | `c0000000-0000-4000-8000-000000000001` |
| Booker Wholesale supplier | `9b000000-0000-4000-8000-000000000001` |

Product, brand, category and customer GUIDs come from the JSON files — they are stable per file but
not worth memorising. Fetch them with `GET /api/v1/catalog/products`.

---

## The seeded scenario

The seed data is not random sample data; it is a deliberate two-organization franchise setup you can
test the whole chain layer against.

```
FluentPOS Retail  (franchisor, 0% royalty)
   └── Store One (Manchester, default store)
          └── Till 1
          └── staff@fluentpos.com   ← store-scoped cashier

Northern Franchise Ltd  (franchisee, 5% royalty)
   └── Store Two (Leeds)
          └── Till 1
   └── franchisee@fluentpos.com     ← org-scoped manager (Manager role)

superadmin@fluentpos.com            ← head office, unscoped, every permission
Booker Wholesale                    ← supplier for purchasing / price-file import
42 products, 3 VAT rates            ← shared central catalog, both stores inherit
```

That gives you, out of the box: two stores under different owners, a central catalog both inherit, a
store-scoped user to prove isolation, an org-scoped user to prove the franchise reporting view, a
royalty rate to accrue against, and a supplier to raise purchase orders with.

The [testing guide](testing-guide.md) walks scenarios through exactly this setup.

---

## What is deliberately *not* seeded

| Missing | Why it matters |
|---|---|
| **Product barcodes** | Products carry a `BarcodeSymbology` name but no EAN value. Wholesaler price-file import matches **by barcode**, so it matches nothing until you set one |
| **`StoreProduct` overlays** | Every store inherits the central price. Create them to test price overrides and reorder points |
| **Stock** | All products start at zero in both stores. Receive a purchase order to create stock |
| **Orders, transactions, till sessions** | Produced by transacting |
| **`DailyStoreSales` rows** | Projected from sales events — empty until you sell something |
| **Age-restricted products** | `IsAgeRestricted` / `MinimumAge` are false/null. Set them to test Challenge 25 |
| **POS PINs and device keys** | Set per user / per terminal — see [users-and-access.md](users-and-access.md#terminal-device-auth-and-pin-sign-in) |

The [testing guide](testing-guide.md) fills all of these in as its setup steps.

---

## Resetting the database

Because seeders only insert into empty tables, a schema or seed-shape change usually means starting
over. There is no production data and no upgrade path from pre-Phase-1 databases — migrations were
squashed to a single `Initial` per context in August 2026.

### Local PostgreSQL

```bash
psql -U postgres -c "DROP DATABASE IF EXISTS fluentpos WITH (FORCE);"
dotnet run --project src/server/API      # recreates and reseeds
```

```powershell
# If psql is not on PATH
& "C:\Program Files\PostgreSQL\16\bin\psql.exe" -U postgres -c "DROP DATABASE IF EXISTS fluentpos WITH (FORCE);"
```

`WITH (FORCE)` (PostgreSQL 13+) drops the database even with the API still holding a connection —
otherwise stop the API first.

### Docker Compose

```bash
docker compose down -v          # -v deletes the postgres-data and redis-data volumes
docker compose up --build
```

### Just one module's data

Drop that module's schema and let its migration re-run:

```sql
DROP SCHEMA catalog CASCADE;
```

Then delete the corresponding rows from `__EFMigrationsHistory` for that context, or the migrator
will think it is already applied.

---

## Turning seeding off

For any environment you do not want sample data in:

```jsonc
"PersistenceSettings": {
  "MigrateOnStartup": false,   // run migrate-database.ps1 as a release step instead
  "SeedOnStartup": false
}
```

Or as environment variables:

```bash
PersistenceSettings__MigrateOnStartup=false
PersistenceSettings__SeedOnStartup=false
```

**With more than one API replica, `MigrateOnStartup` must be false** — concurrent migrators race.
See [deployment.md](deployment.md).

Note that turning seeding off means you get *no* organization and *no* default store. Since
store-scoped inserts resolve the default store, you must create an organization, store and terminal
before anything can transact. Bootstrapping a clean production tenant is a gap worth scripting.

---

## Adding your own seed data

To extend an existing module's seed data, edit the JSON in
`Modules/<Name>/Modules.<Name>.Infrastructure/Persistence/SeedData/` and reset the relevant table —
the seeder skips non-empty tables.

To add a seeder to a new module:

1. Implement `IDatabaseSeeder` (`Shared.Core/Interfaces/Services/IDatabaseSeeder.cs`):

   ```csharp
   internal class MyModuleDbSeeder : IDatabaseSeeder
   {
       public void Initialize()
       {
           if (_db.Things.Any()) return;      // idempotency guard — always include one
           _db.Things.AddRange(/* … */);
           _db.SaveChanges();
       }
   }
   ```

2. Register it in the module's `ModuleExtensions`: `services.AddTransient<IDatabaseSeeder, MyModuleDbSeeder>();`
3. Use **fixed GUIDs** for anything other modules or tests will reference, and put them in
   `Shared.Core/Constants/` if they cross a module boundary — that is the pattern
   `OrganizationConstants` follows.
4. Wrap the body in try/catch and log; a seeder throwing takes the whole application down on boot.

---

## Loading a realistic catalog

The 42 seeded products are laptops and phones — fine for smoke tests, useless for convenience-retail
testing. Two ways to get real data in:

**Via the API**, for full control:

```bash
curl -s -X POST http://localhost:5000/api/v1/catalog/products \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{
        "name": "Coca-Cola 500ml",
        "barcode": "5449000000996",
        "price": 1.35,
        "cost": 0.72,
        "vatRateId": "6f3a1a2b-0000-4000-8000-000000000003",
        "brandId": "<guid>",
        "categoryId": "<guid>",
        "isAgeRestricted": false
      }'
```

`vatRateId` is required and is the single source of truth for tax — DTOs expose a computed `Tax`
percentage derived from it. `barcode` must be unique.

**Via a wholesaler price file**, once products exist with barcodes:

```bash
curl -s -X POST "http://localhost:5000/api/v1/purchasing/suppliers/9b000000-0000-4000-8000-000000000001/import-pricefile" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{ "csv": "barcode,cost,price\n5449000000996,0.68,1.35\n" }'
```

Import **updates** cost/price on products matched by barcode; it does not create products. Unmatched
barcodes come back in `unmatchedBarcodes` so you can see what your catalog is missing.
