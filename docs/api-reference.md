# API Reference

Endpoint map with the permission each one requires. Swagger at `/swagger` is generated from the code
and is always authoritative; this page exists so you can find things without scrolling.

- Base path: `api/v{version}/...`, version defaults to `1`.
- **All routes are lowercase** (`LowercaseUrls = true`), regardless of controller naming.
- Everything requires `Authorization: Bearer <token>` unless marked **anonymous**.
- Permission names in the table are the short form; the actual claim value is
  `Permissions.<Group>.<Action>` (e.g. `Permissions.Products.ViewAll`).
- List endpoints accept `pageNumber` (default 1), `pageSize` (default 10) and `orderBy`, and return
  `PaginatedResult<T>`. `searchString` is available on some filters only — products, brands,
  categories and event logs. Single-item and command endpoints return `Result<T>`.

Which roles hold which permissions: [users-and-access.md](users-and-access.md).

---

## Identity — `api/v1/identity`

### Tokens

| Method | Path | Permission | Notes |
|---|---|---|---|
| POST | `/identity/tokens` | **anonymous** | `{ email, password }` → `{ token, refreshToken, refreshTokenExpiryTime }` |
| POST | `/identity/tokens/refresh` | **anonymous** | `{ token, refreshToken }`. Accepts an *expired* access token |
| POST | `/identity/tokens/pin` | **anonymous** | `{ terminalId, deviceKey, email, pin }` → token scoped to the terminal's store |
| POST | `/identity/tokens/pin/setup` | any authenticated user | `{ pin }` — sets the caller's own POS PIN |

### Account

| Method | Path | Permission |
|---|---|---|
| POST | `/identity/register` | **anonymous** — see note below |
| GET | `/identity/confirm-email` | **anonymous** |
| GET | `/identity/confirm-phone-number` | **anonymous** |
| POST | `/identity/forgot-password` | **anonymous** |
| POST | `/identity/reset-password` | **anonymous** |

**`POST /identity/register`** — `{ firstName, lastName, email, userName, password, confirmPassword,
phoneNumber, emailConfirmed, phoneNumberConfirmed }`. `userName` and `password` need 6+ characters.
`isActive` is set automatically, and **every new user is auto-assigned the `Staff` role**. Set
`emailConfirmed`/`phoneNumberConfirmed` to skip verification locally.

> This endpoint is **anonymous**, so anyone who can reach the API can create a Staff-role account.
> Restrict or disable it before exposing the API publicly.

### Users and roles

| Method | Path | Permission |
|---|---|---|
| GET | `/identity/users` | `Users.View` |
| GET | `/identity/users/{id}` | `Users.View` |
| PUT | `/identity/users` | `Users.Edit` |
| GET | `/identity/users/roles/{id}` | `Users.View` |
| PUT | `/identity/users/roles/{id}` | `Users.Edit` |
| GET | `/identity/roles` | `Roles.View` |
| POST | `/identity/roles` | `Roles.Create` |
| DELETE | `/identity/roles/{id}` | `Roles.Delete` |
| GET | `/identity/roles/permissions` | `RoleClaims.View` |
| GET | `/identity/roles/permissions/{id}` | `RoleClaims.View` |
| GET | `/identity/roles/permissions/byrole/{roleId}` | `RoleClaims.View` |
| PUT | `/identity/roles/permissions/update` | `RoleClaims.Edit` |
| DELETE | `/identity/roles/permissions/{id}` | `RoleClaims.Delete` |

### Event log

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/identity/eventlogs` | `EventLogs.ViewAll` | Every domain event with user attribution |
| POST | `/identity/eventlogs` | any authenticated user | |

### Extended attributes

`GET|POST|PUT|DELETE /identity/user/attributes` and `/identity/role/attributes` —
`Users.ExtendedAttributes.*` / `Roles.ExtendedAttributes.*`. See
[adding-extended-attribute-tutorial.md](adding-extended-attribute-tutorial.md).

---

## Organizations — `api/v1/organization`

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/organization/organizations` | `Organizations.ViewAll` | |
| POST | `/organization/organizations` | `Organizations.Register` | Franchisee onboarding; carries `royaltyRatePercent` |
| GET | `/organization/stores` | `Stores.ViewAll` | |
| GET | `/organization/stores/{id}` | `Stores.View` | |
| POST | `/organization/stores` | `Stores.Register` | |
| PUT | `/organization/stores` | `Stores.Update` | `organizationId` moves a store between orgs |
| DELETE | `/organization/stores/{id}` | `Stores.Remove` | |
| GET | `/organization/terminals` | `Terminals.ViewAll` | |
| POST | `/organization/terminals` | `Terminals.Register` | |
| POST | `/organization/terminals/{id}/register-device` | `Terminals.Register` | Issues a long-lived device key — **shown once**, SHA-256 hash stored. Re-running rotates it |

Request bodies:

```jsonc
// POST /organization/organizations
{ "name": "Southern Franchise Ltd", "detail": "New franchisee", "royaltyRatePercent": 6 }

// POST /organization/stores
{ "organizationId": "<guid|null>", "name": "Store Three",
  "addressLine": "1 High St", "city": "Bristol", "postcode": "BS1 1AA", "phone": "0117 000 0000" }

// PUT /organization/stores — send the whole store; `isActive` defaults to true, so
// omitting it on an inactive store silently reactivates it
{ "id": "<guid>", "name": "Store Three", "addressLine": "...", "city": "Bristol",
  "postcode": "BS1 1AA", "phone": "...", "organizationId": "<guid|null>", "isActive": true }

// POST /organization/terminals
{ "storeId": "<guid>", "name": "Till 2" }
```

Setting `organizationId` on `PUT /organization/stores` is how you move a store between organizations —
i.e. convert a company-owned store into a franchise, or take one back.

---

## Catalog — `api/v1/catalog`

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/catalog/products` | `Products.ViewAll` | Paginated |
| GET | `/catalog/products/{id}` | `Products.View` | |
| GET | `/catalog/products/image/{id}` | `Products.View` | |
| POST | `/catalog/products` | `Products.Register` | `vatRateId` is required; `barcode` (EAN) must be unique |
| PUT | `/catalog/products` | `Products.Update` | |
| DELETE | `/catalog/products/{id}` | `Products.Remove` | |
| GET | `/catalog/brands` | `Brands.ViewAll` | |
| GET | `/catalog/brands/{id}` | `Brands.View` | |
| POST · PUT · DELETE | `/catalog/brands` · `/catalog/brands/{id}` | `Brands.Register` · `Update` · `Remove` | |
| GET | `/catalog/categories` | `Categories.ViewAll` | |
| GET | `/catalog/categories/{id}` | `Categories.View` | |
| POST · PUT · DELETE | `/catalog/categories` · `/catalog/categories/{id}` | `Categories.Register` · `Update` · `Remove` | |
| GET | `/catalog/vatrates` | `VatRates.ViewAll` | UK Zero / Reduced / Standard |
| GET | `/catalog/storeproducts` | `StoreProducts.ViewAll` | The per-store overlay |
| POST | `/catalog/storeproducts` | `StoreProducts.Upsert` | Create or update — see body below |
| DELETE | `/catalog/storeproducts/{id}` | `StoreProducts.Remove` | Store falls back to the central price |
| GET | `/catalog/sync?since=<iso8601>` | `Products.ViewAll` | Incremental node feed |

Extended attributes: `/catalog/product/attributes`, `/catalog/brand/attributes`,
`/catalog/category/attributes`.

**`POST /catalog/storeproducts`**

```json
{
  "storeId": "51000000-0000-4000-8000-000000000002",
  "productId": "<guid>",
  "price": 99.99,
  "isRanged": true,
  "reorderPoint": 5,
  "reorderQuantity": 24,
  "preferredSupplierId": "9b000000-0000-4000-8000-000000000001"
}
```

`price` is the store's sell-price override; omit it to inherit the central price. `isRanged` controls
whether the store carries the product at all. `reorderPoint` / `reorderQuantity` /
`preferredSupplierId` drive auto-replenishment.

**`GET /catalog/sync`** — omit `since` for a full pull. The response is:

```jsonc
{
  "serverTime": "2026-08-08T10:12:00.123Z",   // persist this; pass it as the next ?since=
  "products":      [ /* id, name, barcode, price, cost, vatRate, isAgeRestricted, minimumAge, … */ ],
  "storeProducts": [ /* the caller's store overlays — scoped by the query filter automatically */ ],
  "vatRates":      [ /* id, name, rate */ ]
}
```

`serverTime` is captured **before** the queries run, so a change landing mid-request is re-sent on the
next pull rather than missed. That means occasional duplicates — clients upsert by id, so this is safe.
Cursors are pure server clock, so device clock skew is irrelevant.

---

## People — `api/v1/people`

| Method | Path | Permission |
|---|---|---|
| GET | `/people/customers` | `Customers.ViewAll` |
| GET | `/people/customers/{id}` | `Customers.View` |
| POST · PUT · DELETE | `/people/customers` · `/people/customers/{id}` | `Customers.Register` · `Update` · `Remove` |
| GET | `/people/carts` | `Carts.ViewAll` |
| GET | `/people/carts/{id}` | `Carts.View` |
| POST | `/people/carts` | `Carts.Create` |
| DELETE | `/people/carts/{id}` | `Carts.Remove` |
| DELETE | `/people/carts/clear/{id}` | `Carts.Remove` |
| GET | `/people/cartitems` | `CartItems.ViewAll` |
| GET | `/people/cartitems/{id}` | `CartItems.View` |
| POST · PUT · DELETE | `/people/cartitems` · `/people/cartitems/{id}` | `CartItems.Add` · `Update` · `Remove` |

Extended attributes: `/people/customer/attributes`, `/people/cart/attributes`,
`/people/cartitem/attributes`.

> Server-side carts are the **legacy** checkout path used by the Angular client. The POS client owns
> its basket on the device and uses `POST /sales/orders/pos` instead. Carts are store-scoped: a
> store-scoped user touching another store's cart gets 403.

---

## Sales — `api/v1/sales`

### Orders

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/sales/orders` | `Sales.ViewAll` | Filtered to the caller's store |
| GET | `/sales/orders/{id}` | `Sales.View` | 404 for another store's order |
| POST | `/sales/orders` | `Sales.Register` | Legacy cart checkout — `{ cartId, ... }` |
| POST | `/sales/orders/pos` | `Sales.Register` | Offline-capable checkout, idempotent |
| POST | `/sales/orders/{id}/refund` | `Sales.Refund` | Full-order refund |

**`POST /sales/orders/pos`**

```json
{
  "clientSaleId": "8f2d1c40-0000-4000-8000-0000000000aa",
  "storeId": null,
  "customerId": null,
  "tillSessionId": null,
  "paymentType": 0,
  "tenderedAmount": 480.00,
  "ageVerified": false,
  "occurredAt": "2026-08-08T10:12:00Z",
  "note": null,
  "items": [{ "productId": "<guid>", "quantity": 2 }]
}
```

- `clientSaleId` is generated on the device and **becomes the order id** — resubmitting the same id
  returns the existing order instead of creating a second one.
- `storeId` is optional for store-scoped tokens (their claim wins); head office may specify it.
- `customerId` omitted → the seeded walk-in customer.
- `ageVerified` must be `true` if any line is age-restricted, or checkout is rejected. The
  verification is recorded on the order for licensing audits.
- `occurredAt` is informational; server time stays authoritative.

**`POST /sales/orders/{id}/refund`** — `{ "reason": "...", "tillSessionId": "<guid|null>" }`.
`reason` is mandatory. Reverses the payment as a negative transaction of the original payment type
and returns the goods to stock as `Return` movements.

### Till sessions

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/sales/tillsessions` | `TillSessions.ViewAll` | |
| GET | `/sales/tillsessions/{id}` | `TillSessions.View` | Includes live X-report figures |
| POST | `/sales/tillsessions/open` | `TillSessions.Open` | `{ terminalId, openingFloat, storeId? }` — one open session per till |
| POST | `/sales/tillsessions/{id}/close` | `TillSessions.Close` | `{ countedCash, notes }` — the Z report |
| POST | `/sales/tillsessions/{id}/cash-movements` | `TillSessions.RecordCashMovement` | `{ tillSessionId, kind, amount, reason }` |

Closing computes `expected = openingFloat + cash takings ± movements` and records
`variance = countedCash − expected`.

---

## Inventory

No controllers. Stock is written through `IStockService` by Sales (checkout, refund) and Purchasing
(receiving), and read per store. Movement kinds: `Sale`, `Purchase`, `Return`.

---

## Purchasing — `api/v1/purchasing`

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/purchasing/suppliers` | `Suppliers.ViewAll` | |
| POST · PUT · DELETE | `/purchasing/suppliers` · `/purchasing/suppliers/{id}` | `Suppliers.Register` · `Update` · `Remove` | |
| POST | `/purchasing/suppliers/{id}/import-pricefile` | `Suppliers.ImportPrices` | Booker/Bestway-style CSV |
| GET | `/purchasing/purchaseorders` | `PurchaseOrders.ViewAll` | |
| GET | `/purchasing/purchaseorders/{id}` | `PurchaseOrders.View` | |
| POST | `/purchasing/purchaseorders` | `PurchaseOrders.Register` | Creates a **Draft** |
| POST | `/purchasing/purchaseorders/{id}/submit` | `PurchaseOrders.Update` | Draft → Submitted |
| POST | `/purchasing/purchaseorders/{id}/receive` | `PurchaseOrders.Receive` | Books goods into store stock |
| POST | `/purchasing/purchaseorders/{id}/cancel` | `PurchaseOrders.Update` | |
| GET | `/purchasing/purchaseorders/{id}/export` | `PurchaseOrders.View` | CSV for the wholesaler |
| POST | `/purchasing/replenishment/run` | `Replenishment.Run` | On-demand run of the hourly job |

Purchase order lifecycle: `Draft → Submitted → Received | Cancelled`.

**`POST /purchasing/purchaseorders`**

```json
{
  "storeId": null,
  "supplierId": "9b000000-0000-4000-8000-000000000001",
  "notes": "weekly order",
  "items": [{ "productId": "<guid>", "quantity": 24, "unitCost": 150.00 }]
}
```

`unitCost` is optional and falls back to the product's central cost.

**`POST /purchasing/purchaseorders/{id}/receive`**

```json
{ "items": [{ "productId": "<guid>", "receivedQuantity": 24 }] }
```

Lines you omit are received in full.

**`POST /purchasing/suppliers/{id}/import-pricefile`**

```json
{ "csv": "barcode,cost,price\n5012345678900,142.50,199.00\n" }
```

Lines are `<barcode>,<cost>[,<sellPrice>]`; header rows are skipped. Products are matched **by
barcode**. Returns `{ totalLines, updated, unmatchedBarcodes[], invalidLines[] }`.

> The 42 seeded products carry a `BarcodeSymbology` but **no barcode value**, so a price-file import
> matches nothing until you set `barcode` on a product first.

---

## Reporting — `api/v1/reporting`

| Method | Path | Permission | Query |
|---|---|---|---|
| GET | `/reporting/salesreports/daily` | `Reporting.View` | `from`, `to`, `storeId` |
| GET | `/reporting/salesreports/royalties` | `Reporting.Royalties` | `from`, `to` |

Both scope themselves to the caller automatically:

| Caller | Sees |
|---|---|
| Store-scoped staff | Their store only (enforced by the EF query filter) |
| Franchisee manager (org-scoped) | Every store in their organization |
| Franchisor / SuperAdmin | Everything, grouped by organization |

`daily` returns one row per store per day: orders, gross, tax, refunds, net, organization snapshot.
`royalties` groups by organization: `net × RoyaltyRatePercent`, snapshotted at projection time.

---

## Operational endpoints

| Path | Auth | Notes |
|---|---|---|
| `/swagger` | none | v1 + v2 documents |
| `/health/live` | none | Liveness — no dependency checks |
| `/health/ready` | none | Readiness — includes `ApplicationDbContext` check |
| `/jobs` | **none** | Hangfire dashboard. Protect or block before exposing — [deployment.md](deployment.md) |
| `/files/...` | none | Uploaded product images |
| `/pos` | app-level | Offline-first PWA till |

---

## Errors

`GlobalExceptionHandler` normalises failures:

| Status | When |
|---|---|
| 400 | FluentValidation failure, or a `CustomException` with an explicit status |
| 401 | Missing/expired token |
| 403 | Authenticated but lacking the permission, **or** reaching across a store boundary |
| 404 | Not found — including entities filtered out by tenancy (this is deliberate) |
| 500 | Unhandled; details logged via Serilog |

A store-scoped user asking for another store's order gets **404, not 403**: the global query filter
removes the row before the handler ever sees it.
