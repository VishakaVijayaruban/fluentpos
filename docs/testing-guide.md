# Testing Guide

How to run the automated tests, and step-by-step scenarios for exercising every feature area by hand.
Because the Angular client has no UI for the multi-store features, manual testing is API-driven.

- [Automated tests](#automated-tests)
- [Writing new tests](#writing-new-tests)
- [Setting up for manual testing](#setting-up-for-manual-testing)
- [Scenario 1 — smoke test](#scenario-1--smoke-test)
- [Scenario 2 — store isolation and per-store pricing](#scenario-2--store-isolation-and-per-store-pricing)
- [Scenario 3 — purchasing and auto-replenishment](#scenario-3--purchasing-and-auto-replenishment)
- [Scenario 4 — till session and the Z report](#scenario-4--till-session-and-the-z-report)
- [Scenario 5 — refunds](#scenario-5--refunds)
- [Scenario 6 — Challenge 25 (age-restricted sales)](#scenario-6--challenge-25-age-restricted-sales)
- [Scenario 7 — offline selling and outbox replay](#scenario-7--offline-selling-and-outbox-replay)
- [Scenario 8 — terminal device auth and PIN sign-in](#scenario-8--terminal-device-auth-and-pin-sign-in)
- [Scenario 9 — franchise reporting and royalties](#scenario-9--franchise-reporting-and-royalties)
- [Scenario 10 — wholesaler price file and PO export](#scenario-10--wholesaler-price-file-and-po-export)
- [Testing checklist](#testing-checklist)

---

## Automated tests

```bash
dotnet test src/server/FluentPOS.sln
```

Currently **33 tests across 4 projects**, all passing:

| Project | Tests | Covers |
|---|---|---|
| `Modules.Sales.Core.Tests` | 17 | Till session arithmetic (expected cash, variance), order totals, tax |
| `Modules.Purchasing.Core.Tests` | 6 | Purchase order state machine, replenishment decisions |
| `Modules.Reporting.Core.Tests` | 4 | `DailyStoreSales` projection and royalty accrual |
| `Shared.Infrastructure.Tests` | 6 | Shared infrastructure behaviour |

These are **unit tests against domain logic only**. There is no integration-test project — no
`WebApplicationFactory`, no test database, no HTTP-level coverage. Anything involving EF query
filters, the permission pipeline, or multi-module flows is only verified by the manual scenarios
below. Closing that gap is the single highest-value testing investment available.

Client tests:

```bash
cd src/client
npm test          # Karma/Jasmine, needs Node 14/16
npm run e2e       # Protractor (deprecated)
```

CI (`.github/workflows/dotnet.yml`) runs restore → build → test on .NET 10 for pushes and PRs to
`master`. `angular.yml` builds the client on Node 14.

---

## Writing new tests

Conventions in this repo:

- Unit tests live in `Modules.<Name>.Core.Tests/` mirroring the source folder structure.
- Test classes are named `<ThingUnderTest>Should` (e.g. `TillSessionShould`, `DailyStoreSalesShould`).
- Test methods read as the sentence continuing the class name.
- xUnit + plain assertions; FakeItEasy for mocking.

Add tests when you change domain logic. See `Modules/Sales/Modules.Sales.Core.Tests/Entities.Tests/TillSessionShould.cs`
for the house style.

---

## Setting up for manual testing

Every scenario assumes the API is running with the seeded data
([seed-data.md](seed-data.md)) and these shell variables set.

> Payloads below are written from the command definitions in the source. If one is rejected, check
> the live contract in Swagger — it is generated from the code and always correct.

### bash / Git Bash

```bash
BASE=http://localhost:5000/api/v1

token() {
  curl -s -X POST "$BASE/identity/tokens" -H "Content-Type: application/json" \
    -d "{\"email\":\"$1\",\"password\":\"123Pa\$\$word!\"}" \
    | python -c "import sys,json;print(json.load(sys.stdin)['token'])"
}

ADMIN=$(token superadmin@fluentpos.com)
STAFF=$(token staff@fluentpos.com)
FRAN=$(token franchisee@fluentpos.com)

AH="Authorization: Bearer $ADMIN"
SH="Authorization: Bearer $STAFF"
FH="Authorization: Bearer $FRAN"
JSON="Content-Type: application/json"

STORE_ONE=51000000-0000-4000-8000-000000000001
STORE_TWO=51000000-0000-4000-8000-000000000002
TILL_ONE=71000000-0000-4000-8000-000000000001
SUPPLIER=9b000000-0000-4000-8000-000000000001
STANDARD_VAT=6f3a1a2b-0000-4000-8000-000000000003
```

### PowerShell

```powershell
$BASE = 'http://localhost:5000/api/v1'

function Get-Token($email) {
  $b = @{ email = $email; password = '123Pa$$word!' } | ConvertTo-Json
  (Invoke-RestMethod -Method Post -Uri "$BASE/identity/tokens" -ContentType 'application/json' -Body $b).token
}

$AH = @{ Authorization = "Bearer $(Get-Token 'superadmin@fluentpos.com')" }
$SH = @{ Authorization = "Bearer $(Get-Token 'staff@fluentpos.com')" }
$FH = @{ Authorization = "Bearer $(Get-Token 'franchisee@fluentpos.com')" }

$STORE_ONE    = '51000000-0000-4000-8000-000000000001'
$STORE_TWO    = '51000000-0000-4000-8000-000000000002'
$TILL_ONE     = '71000000-0000-4000-8000-000000000001'
$SUPPLIER     = '9b000000-0000-4000-8000-000000000001'
$STANDARD_VAT = '6f3a1a2b-0000-4000-8000-000000000003'
```

Pick a product to work with:

```bash
curl -s "$BASE/catalog/products?pageNumber=1&pageSize=3" -H "$AH"
# copy an id into:
PRODUCT=<guid>
```

---

## Scenario 1 — smoke test

Confirms the stack is healthy and auth works. Two minutes.

```bash
curl -s http://localhost:5000/health/live      # Healthy
curl -s http://localhost:5000/health/ready     # Healthy — database reachable

curl -s "$BASE/catalog/products?pageNumber=1&pageSize=5" -H "$AH"   # 42 total
curl -s "$BASE/catalog/vatrates"               -H "$AH"             # Zero / Reduced / Standard
curl -s "$BASE/organization/stores"            -H "$AH"             # Store One, Store Two
curl -s "$BASE/organization/terminals"         -H "$AH"             # Till 1 × 2
curl -s "$BASE/purchasing/suppliers"           -H "$AH"             # Booker Wholesale
```

**Expected:** both health endpoints `Healthy`; 42 products; 2 stores; 2 terminals; 1 supplier.

Negative checks that prove authorization is on:

```bash
curl -s -o /dev/null -w "%{http_code}\n" "$BASE/catalog/products"                 # 401 — no token
curl -s -o /dev/null -w "%{http_code}\n" "$BASE/organization/organizations" -H "$SH"  # 403 — staff lacks Organizations.ViewAll
```

---

## Scenario 2 — store isolation and per-store pricing

The core tenancy guarantee: one central product, two stores, different prices, invisible to each
other.

```bash
# 1. Store Two overrides the price to 99.99
curl -s -X POST "$BASE/catalog/storeproducts" -H "$AH" -H "$JSON" -d "{
  \"storeId\":\"$STORE_TWO\", \"productId\":\"$PRODUCT\",
  \"price\":99.99, \"isRanged\":true
}"

# 2. Sell the product in Store One (staff token — store-scoped)
curl -s -X POST "$BASE/sales/orders/pos" -H "$SH" -H "$JSON" -d "{
  \"clientSaleId\":\"$(uuidgen)\", \"paymentType\":0, \"tenderedAmount\":500,
  \"items\":[{\"productId\":\"$PRODUCT\",\"quantity\":1}]
}"

# 3. Sell the same product in Store Two (admin token, store named explicitly)
curl -s -X POST "$BASE/sales/orders/pos" -H "$AH" -H "$JSON" -d "{
  \"clientSaleId\":\"$(uuidgen)\", \"storeId\":\"$STORE_TWO\",
  \"paymentType\":0, \"tenderedAmount\":200,
  \"items\":[{\"productId\":\"$PRODUCT\",\"quantity\":1}]
}"

# 4. Compare what each caller sees
curl -s "$BASE/sales/orders?pageNumber=1&pageSize=20" -H "$AH"   # both orders
curl -s "$BASE/sales/orders?pageNumber=1&pageSize=20" -H "$SH"   # Store One's only
```

**Expected:**

- The Store Two order is priced at **99.99**; the Store One order at the central price.
- Admin sees both orders; staff sees only Store One's.
- Fetching the Store Two order id as staff returns **404** — the query filter removes it, so the
  handler never sees the row.
- Attempting to touch another store's cart as staff returns **403**.

Stock is independent per store — receive goods in Scenario 3 and confirm Store One's stock does not
move when Store Two receives.

Removing the overlay restores inheritance:

```bash
curl -s -X DELETE "$BASE/catalog/storeproducts/<overlayId>" -H "$AH"
```

---

## Scenario 3 — purchasing and auto-replenishment

Draft PO → submit → receive → stock, then let the replenishment engine do it for you.

### Manual purchase order

```bash
# Create a draft
PO=$(curl -s -X POST "$BASE/purchasing/purchaseorders" -H "$AH" -H "$JSON" -d "{
  \"storeId\":\"$STORE_ONE\", \"supplierId\":\"$SUPPLIER\", \"notes\":\"manual test\",
  \"items\":[{\"productId\":\"$PRODUCT\",\"quantity\":24,\"unitCost\":150.00}]
}")
echo "$PO"          # grab the returned id

curl -s -X POST "$BASE/purchasing/purchaseorders/<poId>/submit"  -H "$AH"
curl -s -X POST "$BASE/purchasing/purchaseorders/<poId>/receive" -H "$AH" -H "$JSON" \
     -d "{\"items\":[{\"productId\":\"$PRODUCT\",\"receivedQuantity\":24}]}"

curl -s "$BASE/purchasing/purchaseorders/<poId>" -H "$AH"     # status Received
```

**Expected:** state moves `Draft → Submitted → Received`; 24 units book into **Store One's** stock as
a `Purchase` movement; Store Two's stock is untouched. Receiving twice, or receiving a Draft, is
rejected.

### Auto-replenishment

```bash
# Give the product a reorder point in Store One
curl -s -X POST "$BASE/catalog/storeproducts" -H "$AH" -H "$JSON" -d "{
  \"storeId\":\"$STORE_ONE\", \"productId\":\"$PRODUCT\", \"isRanged\":true,
  \"reorderPoint\":5, \"reorderQuantity\":24, \"preferredSupplierId\":\"$SUPPLIER\"
}"

# Run the job on demand instead of waiting for the hourly schedule
curl -s -X POST "$BASE/purchasing/replenishment/run" -H "$AH"
curl -s "$BASE/purchasing/purchaseorders?pageNumber=1&pageSize=20" -H "$AH"

# Run it again — this is the important assertion
curl -s -X POST "$BASE/purchasing/replenishment/run" -H "$AH"
```

**Expected:** the first run creates exactly one **draft** PO for Store One under Booker Wholesale for
24 units (24 × £150 = £3,600 in the recorded reference run). The second run creates **nothing** — the
engine skips products already on an open PO. That idempotency is the property worth guarding.

Also visible in the Hangfire dashboard at <http://localhost:5000/jobs> as an hourly recurring job.

---

## Scenario 4 — till session and the Z report

Cash reconciliation end to end.

> `TillSessions.Open` is **not** in the seeded Staff permission set. Use the admin token, or grant
> Staff the permission first (see
> [users-and-access.md](users-and-access.md#granting-permissions-to-a-role)).

```bash
# 1. Open the till with a £50 float
SESSION=$(curl -s -X POST "$BASE/sales/tillsessions/open" -H "$AH" -H "$JSON" -d "{
  \"terminalId\":\"$TILL_ONE\", \"openingFloat\":50.00, \"storeId\":\"$STORE_ONE\"
}")

# 2. Sell against the session
curl -s -X POST "$BASE/sales/orders/pos" -H "$AH" -H "$JSON" -d "{
  \"clientSaleId\":\"$(uuidgen)\", \"storeId\":\"$STORE_ONE\", \"tillSessionId\":\"<sessionId>\",
  \"paymentType\":0, \"tenderedAmount\":480,
  \"items\":[{\"productId\":\"$PRODUCT\",\"quantity\":2}]
}"

# 3. X report — live figures, session still open
curl -s "$BASE/sales/tillsessions/<sessionId>" -H "$AH"

# 4. Pay £20 out of the drawer
curl -s -X POST "$BASE/sales/tillsessions/<sessionId>/cash-movements" -H "$AH" -H "$JSON" -d '{
  "tillSessionId":"<sessionId>", "kind":1, "amount":20.00, "reason":"window cleaner"
}'

# 5. Close it — count £25 into the bag
curl -s -X POST "$BASE/sales/tillsessions/<sessionId>/close" -H "$AH" -H "$JSON" \
     -d '{ "countedCash": 25.00, "notes": "end of shift" }'
```

**Expected:** `expected = float + cash takings ± movements`, and `variance = counted − expected`.
The reference run in the transformation plan: float £50 + cash £480 − refund £480 − payout £20 →
expected £30, counted £25, **variance −£5**.

Also check: opening a second session on the same terminal while one is open is rejected — one open
session per till.

---

## Scenario 5 — refunds

```bash
curl -s -X POST "$BASE/sales/orders/<orderId>/refund" -H "$AH" -H "$JSON" -d '{
  "reason": "customer changed their mind",
  "tillSessionId": "<sessionId>"
}'
```

**Expected:**

- A **negative** transaction of the original payment type — the payment is reversed, not deleted.
- Goods return to that store's stock as `Return` movements.
- An `OrderRefunded` event projects into `DailyStoreSales`, moving the store's refunds and net.
- Omitting `reason` is rejected — it is mandatory for audit.
- The refund reduces cash takings on the till session it is attributed to.

Stock ledger after Scenario 3 + a 2-unit sale + this refund: `+24 Purchase, −2 Sale, +2 Return`.

Refunds are **full-order only** today. Partial refunds are on the backlog.

---

## Scenario 6 — Challenge 25 (age-restricted sales)

```bash
# 1. Flag a product as age-restricted (PUT sends the whole product — GET it first)
curl -s "$BASE/catalog/products/$PRODUCT" -H "$AH"
curl -s -X PUT "$BASE/catalog/products" -H "$AH" -H "$JSON" -d "{
  \"id\":\"$PRODUCT\", \"name\":\"...\", \"price\":200, \"cost\":150,
  \"vatRateId\":\"$STANDARD_VAT\", \"brandId\":\"<guid>\", \"categoryId\":\"<guid>\",
  \"isAgeRestricted\":true, \"minimumAge\":18
}"

# 2. Sell it without verifying — must fail
curl -s -X POST "$BASE/sales/orders/pos" -H "$AH" -H "$JSON" -d "{
  \"clientSaleId\":\"$(uuidgen)\", \"storeId\":\"$STORE_ONE\", \"paymentType\":0,
  \"tenderedAmount\":250, \"ageVerified\":false,
  \"items\":[{\"productId\":\"$PRODUCT\",\"quantity\":1}]
}"

# 3. Sell it with verification — must succeed
curl -s -X POST "$BASE/sales/orders/pos" -H "$AH" -H "$JSON" -d "{
  \"clientSaleId\":\"$(uuidgen)\", \"storeId\":\"$STORE_ONE\", \"paymentType\":0,
  \"tenderedAmount\":250, \"ageVerified\":true,
  \"items\":[{\"productId\":\"$PRODUCT\",\"quantity\":1}]
}"

# 4. Confirm the verification was recorded on the order
curl -s "$BASE/sales/orders/<orderId>" -H "$AH"
```

**Expected:** step 2 is rejected with a validation error; step 3 succeeds and the order carries
`AgeVerificationCompleted` for licensing audits. In the PWA, a restricted basket raises a
confirmation prompt before checkout.

---

## Scenario 7 — offline selling and outbox replay

The scenario that proves the store-node design. Browser only.

1. Open <http://localhost:5000/pos> and sign in as `staff@fluentpos.com` / `123Pa$$word!`.
2. Wait for the catalog to sync — you should see 42 product tiles.
3. Make an **online** sale. Confirm it appears in `GET /sales/orders`.
4. **Stop the API entirely** (Ctrl-C, or `docker compose stop api`).
5. **Reload the page.** It still loads — the service worker serves the app shell and the cached
   catalog from IndexedDB. All 42 tiles are still there.
6. Make a sale while offline. The UI shows **"1 queued"**.
7. **Restart the API.**
8. The outbox drains automatically (on the browser's `online` event, or within the 15-second retry).
   The queued indicator clears.
9. `GET /sales/orders` — the offline sale is present **exactly once**.

### Verifying sync and idempotency directly

```bash
# Full pull
curl -s "$BASE/catalog/sync" -H "$SH"                       # 42 products + overlays + VAT rates + serverTime

# Incremental with nothing changed
curl -s "$BASE/catalog/sync?since=<serverTime>" -H "$SH"    # zero changes

# Change one price, then pull again
curl -s -X PUT "$BASE/catalog/products" -H "$AH" -H "$JSON" -d '{ ... "price": 210 ... }'
curl -s "$BASE/catalog/sync?since=<serverTime>" -H "$SH"    # exactly one change

# Idempotency: submit the same clientSaleId twice
SALE=$(uuidgen)
for i in 1 2; do
  curl -s -X POST "$BASE/sales/orders/pos" -H "$SH" -H "$JSON" -d "{
    \"clientSaleId\":\"$SALE\", \"paymentType\":0, \"tenderedAmount\":300,
    \"items\":[{\"productId\":\"$PRODUCT\",\"quantity\":1}]
  }"
done
curl -s "$BASE/sales/orders?pageNumber=1&pageSize=50" -H "$SH"   # one order, not two
```

**Expected:** both POSTs return the **same** order id, and only one order exists. Cursors are pure
server clock, so device clock skew does not affect the feed.

### Resetting the PWA

Browser DevTools → Application → Storage → *Clear site data*. That drops the IndexedDB caches, the
outbox, and the service worker.

---

## Scenario 8 — terminal device auth and PIN sign-in

```bash
# 1. As admin, register the device (key is returned ONCE)
curl -s -X POST "$BASE/organization/terminals/$TILL_ONE/register-device" -H "$AH"
DEVICE_KEY=<key from the response>

# 2. As the operator, set a PIN
curl -s -X POST "$BASE/identity/tokens/pin/setup" -H "$SH" -H "$JSON" -d '{ "pin": "4821" }'

# 3. Sign in at the till
curl -s -X POST "$BASE/identity/tokens/pin" -H "$JSON" -d "{
  \"terminalId\":\"$TILL_ONE\", \"deviceKey\":\"$DEVICE_KEY\",
  \"email\":\"staff@fluentpos.com\", \"pin\":\"4821\"
}"
```

**Expected:**

- A token scoped to **the terminal's store**, regardless of the user's own `StoreId`.
- Wrong PIN → rejected. Bogus device key → rejected.
- A head-office user signing in at this till gets a **store-scoped** token for that shift.
- A Store One user signing in at Store Two's terminal is **rejected**.
- Re-running step 1 **rotates** the key and invalidates the old one.

Decode the resulting token at <https://jwt.io> and confirm the `storeId` claim.

---

## Scenario 9 — franchise reporting and royalties

Run Scenarios 2 and 5 first so there are sales in both stores plus a refund.

```bash
curl -s "$BASE/reporting/salesreports/daily"     -H "$AH"   # every store
curl -s "$BASE/reporting/salesreports/daily"     -H "$FH"   # franchisee's org only
curl -s "$BASE/reporting/salesreports/daily"     -H "$SH"   # Store One only

curl -s "$BASE/reporting/salesreports/royalties" -H "$AH"   # all orgs
curl -s "$BASE/reporting/salesreports/royalties" -H "$FH"   # their own org only

curl -s "$BASE/reporting/salesreports/daily?from=2026-08-01&to=2026-08-31&storeId=$STORE_TWO" -H "$AH"
```

**Expected:** one `DailyStoreSales` row per store per day with orders, gross, tax, refunds, net and an
organization snapshot. Royalty is `net × RoyaltyRatePercent`, snapshotted at projection time so a
later rate change never rewrites history. The reference run: Northern Franchise — 2 orders, £480
gross, £240 refunded → **£240 net → £12 royalty at 5%**.

Scoping to verify:

| Caller | daily | royalties |
|---|---|---|
| `superadmin` | all stores | all organizations |
| `franchisee` | their org's stores | their org only |
| `staff` | their store's row | 403 — no `Reporting.Royalties` |

Also worth confirming: a reporting failure must never fail a sale. Projection errors are logged and
swallowed by design.

---

## Scenario 10 — wholesaler price file and PO export

```bash
# 1. Give a product a barcode — seeded products have none, so imports match nothing without this
curl -s -X PUT "$BASE/catalog/products" -H "$AH" -H "$JSON" \
     -d "{ \"id\":\"$PRODUCT\", ..., \"barcode\":\"5012345678900\" }"

# 2. Import a Booker/Bestway-style price file, including a bad line on purpose
curl -s -X POST "$BASE/purchasing/suppliers/$SUPPLIER/import-pricefile" -H "$AH" -H "$JSON" -d '{
  "csv": "barcode,cost,price\n5012345678900,142.50,199.00\n9999999999999,1.00,2.00\n"
}'

# 3. Export a purchase order for the wholesaler
curl -s "$BASE/purchasing/purchaseorders/<poId>/export" -H "$AH"
```

**Expected import summary:**

```json
{
  "totalLines": 3,
  "updated": 1,
  "unmatchedBarcodes": ["9999999999999"],
  "invalidLines": ["barcode,cost,price"]
}
```

The header row is reported as invalid (and skipped), the unknown barcode is flagged, and the matched
product's cost becomes 142.50 and price 199.00. Confirm with `GET /catalog/products/{id}`.

The export returns CSV with barcode, product name, quantity and cost — one line per PO line.

---

## Testing checklist

Use this before merging anything that touches tenancy, checkout, or stock.

**Automated**
- [ ] `dotnet build src/server/FluentPOS.sln` — 0 errors
- [ ] `dotnet test src/server/FluentPOS.sln` — 33+ passing
- [ ] New/changed domain logic has unit tests

**Tenancy** (the easiest thing to break)
- [ ] A store-scoped user cannot read another store's orders (404) or carts (403)
- [ ] Stock rows are independent per store
- [ ] A store price override is honoured at checkout; removing it restores inheritance
- [ ] A cached `GetById` response does not leak across stores
- [ ] Reporting scopes correctly for store staff / franchisee manager / franchisor

**Retail operations**
- [ ] PO lifecycle `Draft → Submitted → Received`; receiving books stock into the right store
- [ ] Replenishment is idempotent — a second run adds nothing
- [ ] Till close arithmetic: expected cash and variance
- [ ] Refund reverses payment *and* returns stock
- [ ] Age-restricted basket blocked without `ageVerified`, recorded with it

**Store node**
- [ ] Full sync, then a zero-change incremental, then exactly-one-change after an edit
- [ ] Same `clientSaleId` twice → one order
- [ ] PWA loads and sells with the API stopped; outbox drains on restart, sale lands once
- [ ] PIN + device-key sign-in issues a terminal-store-scoped token; wrong PIN and bad key rejected

**Regression traps**
- [ ] No `DateTime.Now` in anything persisted — PostgreSQL `timestamptz` needs `UtcNow`
- [ ] No `OrderBy` after `ProjectTo` in queries (untranslatable)
- [ ] New store-scoped entities implement `IMustHaveStore`
- [ ] New sync-relevant entities implement `ISyncTracked`
