# Users, Roles and Access Control

Who can do what, how tokens are scoped, and how to set up new users and tills.

- [The three layers of access](#the-three-layers-of-access)
- [Seeded users](#seeded-users)
- [Roles and what they can actually do](#roles-and-what-they-can-actually-do)
- [Permission catalogue](#permission-catalogue)
- [What is in a token](#what-is-in-a-token)
- [How scoping changes what you see](#how-scoping-changes-what-you-see)
- [Creating a user](#creating-a-user)
- [Granting permissions to a role](#granting-permissions-to-a-role)
- [Assigning a user to a store or organization](#assigning-a-user-to-a-store-or-organization)
- [Terminal device auth and PIN sign-in](#terminal-device-auth-and-pin-sign-in)
- [Onboarding a franchisee](#onboarding-a-franchisee)
- [Security notes](#security-notes)

---

## The three layers of access

Every request is filtered three times. Confusing these is the most common source of "why can't I see
my data?".

| Layer | Mechanism | Failure looks like |
|---|---|---|
| **1. Authentication** | JWT bearer token from `/identity/tokens` | 401 |
| **2. Permission** | Claim-based policy on the endpoint (`Permissions.Products.ViewAll`) | 403 |
| **3. Tenancy** | EF Core global query filter on the caller's `storeId` / `orgId` | 404, or a silently shorter list |

Layer 3 is the one that surprises people. A user with every permission in the world still only sees
their own store's orders if their token carries a `storeId`.

---

## Seeded users

All three share the password **`123Pa$$word!`** (`UserConstants.DefaultPassword`).

| Email | Username | Name | Role | Scope | Use it for |
|---|---|---|---|---|---|
| `superadmin@fluentpos.com` | `superadmin` | Mukesh Murugan | SuperAdmin | Head office — no store, no org | Everything; the only account that can run admin/setup calls |
| `staff@fluentpos.com` | `staff` | John Doe | Staff | **Store One** (`51000000-…-0001`) | Testing till operation and store isolation |
| `franchisee@fluentpos.com` | `franchisee` | Farah North | Manager | **Northern Franchise Ltd** org (`7a000000-…-0002`), no store | Testing the franchise reporting view |

> Change these before any deployment that is reachable by anyone else. They are hardcoded in
> `IdentityDbSeeder` and are public knowledge.

---

## Roles and what they can actually do

Six roles are seeded. **Only three of them get any permissions.** This trips people up constantly:

| Role | Permissions granted at seed | Effective ability |
|---|---|---|
| **SuperAdmin** | *Every* permission (reflected off the `Permissions` class) | Full control |
| **Staff** | The POS set — see below | Sell in their store |
| **Manager** | The franchise/reporting set — see below | Read-only chain oversight |
| **Admin** | **none** | Can log in; every endpoint returns 403 |
| **Accountant** | **none** | Same |
| **Cashier** | **none** | Same |

Admin, Accountant and Cashier exist as names only. Grant them claims via
`PUT /identity/roles/permissions/update` before using them, or treat them as placeholders.

### Staff (the POS set)

Products `View`/`ViewAll` · Brands `View`/`ViewAll` · Categories `View`/`ViewAll` ·
VatRates `ViewAll` · Customers `View`/`ViewAll`/`Register` · Carts `View`/`ViewAll`/`Create`/`Remove` ·
CartItems `View`/`ViewAll`/`Add`/`Update`/`Remove` · Sales `View`/`ViewAll`/`Register` ·
Reporting `View`

Enough to run the till and see their own store's numbers. **Notably absent:** `TillSessions.Open`,
`Sales.Refund`, and anything in Purchasing — a seeded staff user cannot open a till session or issue
a refund. Grant those explicitly if that is the workflow you want.

### Manager (the franchise set)

Reporting `View`/`Royalties` · Sales `View`/`ViewAll`/`Refund` · Stores `View`/`ViewAll` ·
TillSessions `View`/`ViewAll` · Products `View`/`ViewAll`

Read-only oversight plus refunds. No catalog editing, no purchasing.

---

## Permission catalogue

Claim values are `Permissions.<Group>.<Action>`, defined in
`src/server/Shared/Shared.Core/Constants/Permissions.cs`.

| Group | Actions |
|---|---|
| `Users` | View · Create · Edit · Delete |
| `Roles` | View · Create · Edit · Delete |
| `RoleClaims` | View · Create · Edit · Delete |
| `Organizations` | ViewAll · Register · Update |
| `Stores` | View · ViewAll · Register · Update · Remove |
| `Terminals` | ViewAll · Register |
| `Brands` · `Categories` · `Products` · `Customers` | View · ViewAll · Register · Update · Remove |
| `VatRates` | ViewAll |
| `StoreProducts` | ViewAll · Upsert · Remove |
| `Carts` | View · ViewAll · Create · Remove |
| `CartItems` | View · ViewAll · Add · Update · Remove |
| `Sales` | View · ViewAll · Register · Refund |
| `TillSessions` | View · ViewAll · Open · Close · RecordCashMovement |
| `Suppliers` | View · ViewAll · Register · Update · Remove · ImportPrices |
| `PurchaseOrders` | View · ViewAll · Register · Update · Receive · Remove |
| `Replenishment` | Run |
| `Reporting` | View · Royalties |
| `EventLogs` | ViewAll |
| `<Entity>ExtendedAttributes` | View · ViewAll · Add · Update · Remove |

Enforcement is `PermissionPolicyProvider` + `PermissionAuthorizationHandler` in
`Shared.Infrastructure` — policies are resolved dynamically from the string, so adding a permission
means adding a constant and referencing it in `[Authorize(Policy = ...)]`.

---

## What is in a token

`TokenService` issues a JWT carrying:

| Claim | Meaning |
|---|---|
| `sub` / `nameidentifier` | User id |
| `email`, `given_name`, `family_name` | Profile |
| `role` | One entry per assigned role |
| `Permission` | One entry per permission claim on those roles |
| `storeId` | The user's store. **Absent = head office (unscoped)** |
| `orgId` | The user's organization. Absent = franchisor/head office |

Lifetime: 60 minutes access, 7 days refresh (`JwtSettings`). Validation is strict — issuer,
audience, lifetime, and signing key are all checked. `/identity/tokens/refresh` deliberately accepts
an **expired** access token paired with a valid refresh token.

Inspect a token at <https://jwt.io> when debugging a scoping problem — it is almost always a missing
or unexpected `storeId`.

---

## How scoping changes what you see

Same endpoint, three callers:

| `GET /api/v1/sales/orders` as… | Result |
|---|---|
| `superadmin` (no `storeId`) | Orders from every store |
| `staff` (`storeId` = Store One) | Store One's orders only |
| `franchisee` (`orgId` = Northern Franchise) | Northern Franchise's stores |

And on reporting:

| `GET /api/v1/reporting/salesreports/daily` as… | Result |
|---|---|
| `staff` | One row — Store One |
| `franchisee` | Rows for their organization's stores |
| `superadmin` | Every store, grouped by organization |

Asking for an entity outside your scope returns **404**, not 403 — the query filter removes the row
before the handler runs. Attempting to *write* across the boundary (e.g. another store's cart)
returns **403**.

Head-office users (no `storeId`) are unscoped for reads and **transact against the default store**
when a command does not name one. Pass `storeId` explicitly on commands when acting as head office.

---

## Creating a user

There is no "create user" admin endpoint — registration is the self-service path, then you assign
roles.

### 1. Register

```bash
curl -s -X POST http://localhost:5000/api/v1/identity/register \
  -H "Content-Type: application/json" \
  -d '{
        "firstName": "Ada",
        "lastName": "Lovelace",
        "email": "ada@fluentpos.com",
        "userName": "ada",
        "password": "123Pa$$word!",
        "confirmPassword": "123Pa$$word!",
        "phoneNumber": "0700000000",
        "emailConfirmed": true,
        "phoneNumberConfirmed": true
      }'
```

Notes on the actual behaviour of this endpoint (`IdentityService.RegisterAsync`):

- `emailConfirmed: true` / `phoneNumberConfirmed: true` skip the verification round trip — useful
  locally, since `MailSettings` points at a sample Ethereal inbox. Leave them off (with
  `MailSettings.EnableVerification` on and real SMTP configured) and the user must confirm first.
- `userName` and `password` both require a **minimum of 6 characters**; `confirmPassword` must match.
- `isActive` is set to `true` automatically — there is no request field for it.
- **Every new user is automatically added to the `Staff` role.** If you want a different role, replace
  the assignment in step 3 rather than only adding to it.
- Duplicate username, email or phone number are each rejected with a distinct error.

### 2. Find the user id

```bash
curl -s "http://localhost:5000/api/v1/identity/users" -H "Authorization: Bearer $TOKEN"
```

### 3. Assign roles

Fetch the current shape first — `GET /identity/users/roles/{id}` returns **every** role with a
`selected` flag:

```bash
curl -s "http://localhost:5000/api/v1/identity/users/roles/<userId>" -H "Authorization: Bearer $TOKEN"
```

Send that same list back with the flags you want (`UserRolesRequest` carries only `userRoles`; the user
id comes from the route):

```bash
curl -s -X PUT "http://localhost:5000/api/v1/identity/users/roles/<userId>" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{ "userRoles": [
          { "roleId": "<roleId>", "roleName": "Staff",   "selected": true  },
          { "roleId": "<roleId>", "roleName": "Manager",  "selected": false }
        ] }'
```

Send the **full** list, not just the entries you are changing — anything marked `selected: false` is
removed. Remember new users are auto-assigned `Staff`, so deselect it if that is not what you want.

### 4. Have them sign in

Roles and permissions are baked into the token at issuance, so the user must obtain a **new** token
after any role change.

---

## Granting permissions to a role

Same pattern as roles: read the full list, flip flags, send it all back.

```bash
# What does this role hold today? Returns every permission with a `selected` flag.
curl -s "http://localhost:5000/api/v1/identity/roles/permissions/byrole/<roleId>" \
  -H "Authorization: Bearer $TOKEN"

# Send the full list back with `selected` toggled (PermissionRequest = { roleId, roleClaims })
curl -s -X PUT "http://localhost:5000/api/v1/identity/roles/permissions/update" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{ "roleId": "<roleId>",
        "roleClaims": [
          { "type": "Permission", "value": "Permissions.TillSessions.Open",  "selected": true },
          { "type": "Permission", "value": "Permissions.TillSessions.Close", "selected": true },
          { "type": "Permission", "value": "Permissions.Sales.Refund",       "selected": true }
        ] }'
```

Each `roleClaims` entry is a `RoleClaimModel` (`id`, `roleId`, `type`, `value`, `description`,
`group`, `selected`). `type` is always `"Permission"`. Send the whole set — omitted or
`selected: false` claims are removed.

This is how you make the `Admin`, `Accountant` and `Cashier` roles useful, and how you give Staff
`TillSessions.Open` / `Sales.Refund` if your store workflow needs it.

---

## Assigning a user to a store or organization

**There is no API for this yet** — a known gap. `FluentUser.StoreId` and `FluentUser.OrganizationId`
are set by the seeder or directly in the database:

```sql
-- Scope a user to Store Two
UPDATE identity."Users"
SET    "StoreId" = '51000000-0000-4000-8000-000000000002'
WHERE  "Email"   = 'ada@fluentpos.com';

-- Make a user an organization-level (franchisee) user with no single store
UPDATE identity."Users"
SET    "StoreId" = NULL,
       "OrganizationId" = '7a000000-0000-4000-8000-000000000002'
WHERE  "Email"   = 'ada@fluentpos.com';
```

Verify your schema/table names against your database — quoting is case-sensitive in PostgreSQL. The
change takes effect on the user's **next** token.

Adding a proper `PUT /identity/users/{id}/store` endpoint is on the backlog.

---

## Terminal device auth and PIN sign-in

For tills you do not want to type an email and password at, the API supports a two-factor device
model: a long-lived **device key** identifies the till, a short **PIN** identifies the operator, and
the resulting token is always scoped to that terminal's store.

### 1. Register the device (once per till, as an admin)

```bash
curl -s -X POST "http://localhost:5000/api/v1/organization/terminals/<terminalId>/register-device" \
  -H "Authorization: Bearer $TOKEN"
```

Returns the device key **once**. Only a SHA-256 hash is stored, so there is no way to read it back —
re-running the call rotates the key and invalidates the old one. Store it in the till's local
storage / keychain.

### 2. The operator sets a PIN (as themselves, once)

```bash
curl -s -X POST "http://localhost:5000/api/v1/identity/tokens/pin/setup" \
  -H "Authorization: Bearer $USER_TOKEN" -H "Content-Type: application/json" \
  -d '{ "pin": "4821" }'
```

### 3. Sign in at the till

```bash
curl -s -X POST "http://localhost:5000/api/v1/identity/tokens/pin" \
  -H "Content-Type: application/json" \
  -d '{
        "terminalId": "71000000-0000-4000-8000-000000000001",
        "deviceKey": "<key from step 1>",
        "email": "staff@fluentpos.com",
        "pin": "4821"
      }'
```

The token is scoped to the terminal's store regardless of the user's own `StoreId`:

- A **head-office** user signing in at a till acts store-scoped for that shift.
- A **store staff** user **cannot** sign in at another store's terminal.

Wrong PIN and bogus device key are both rejected.

> The bundled PWA at `/pos` currently signs in with email + password, not this flow. Wiring it up is
> a small, well-scoped task.

---

## Onboarding a franchisee

The full sequence, as SuperAdmin:

```bash
# 1. Create the franchisee organization with its royalty rate
curl -s -X POST "http://localhost:5000/api/v1/organization/organizations" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{ "name": "Southern Franchise Ltd", "detail": "New franchisee", "royaltyRatePercent": 6 }'

# 2. Create their store under that organization
curl -s -X POST "http://localhost:5000/api/v1/organization/stores" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{ "name": "Store Three", "city": "Bristol", "organizationId": "<orgId>" }'

# 3. Register a till
curl -s -X POST "http://localhost:5000/api/v1/organization/terminals" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{ "name": "Till 1", "storeId": "<storeId>" }'
```

Then register their manager (see [Creating a user](#creating-a-user)), give them the **Manager**
role, and set `OrganizationId` on the row (see above). Register cashiers the same way with
**Staff** and a `StoreId`.

Optionally add per-store pricing with `POST /catalog/storeproducts`, and reorder points so
auto-replenishment starts generating draft POs for them.

An existing store can be moved between organizations with `PUT /organization/stores` by setting
`organizationId` — that is how you convert a company-owned store into a franchise.

---

## Security notes

Before this is reachable by anyone but you:

1. **Change every seeded password**, or delete the seeded users and set `SeedOnStartup = false`.
2. **Replace `JwtSettings.Key`.** The sample value is in source control. Use 32+ random characters
   from a secrets store, injected as `JwtSettings__Key`.
3. **Turn on `RequireHttpsMetadata`** and terminate TLS properly.
4. **Protect `/jobs`.** The Hangfire dashboard has no authorization filter — anyone who can reach it
   can trigger and inspect jobs.
5. **Restrict `POST /identity/register`.** It is anonymous *and* auto-assigns the `Staff` role, so
   anyone who can reach the API can create an account that can sell. Put it behind a permission,
   remove it, or gate it at the proxy.
6. **Review the empty roles.** `Admin`, `Accountant` and `Cashier` grant nothing today; if someone
   later bulk-assigns claims to them, everyone holding those roles gains them at once.
7. **Replace `MailSettings` / `SmsSettings`.** They ship with sample Ethereal credentials.

Full checklist: [deployment.md](deployment.md).
