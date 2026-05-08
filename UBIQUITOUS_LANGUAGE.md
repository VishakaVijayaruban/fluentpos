# Ubiquitous Language — FluentPOS

> **Last updated:** <!-- YYYY-MM-DD -->
>
> This document is the single source of truth for domain terminology in FluentPOS. Every engineer, designer, product owner, and stakeholder should use these terms — in code, tests, pull requests, issues, and conversations — without substitution.

---

## 1. Introduction

**Ubiquitous Language** is a shared vocabulary, built jointly by developers and domain experts, that is used consistently across the entire project — from database column names to API routes to Slack messages. When everyone uses the same terms for the same concepts, misunderstandings vanish and the code becomes self-documenting.

In FluentPOS, this matters because:

- The system spans multiple bounded contexts (Catalog, Sales, People, Inventory, Identity) that each have their own meaning for overlapping words (e.g., "product" means something different in Catalog vs. in a Sales order line).
- Angular clients, REST controllers, MediatR handlers, and EF Core entities must all speak the same language so that a bug report ("the cart item quantity is wrong") maps unambiguously to code.
- New contributors should be able to read a term in a GitHub issue and find the exact class in the codebase within seconds.

---

## 2. Bounded Contexts

A **Bounded Context** is a linguistic boundary inside which a specific domain model applies. The same word may have a different meaning in a different context — that is intentional and correct.

| # | Bounded Context | Short Description | Module / Namespace |
|---|---|---|---|
| 1 | **Catalog** | Master data for what the store sells — products, brands, and categories | `src/server/Modules/Catalog` · `FluentPOS.Modules.Catalog.*` |
| 2 | **Sales** | Completed point-of-sale transactions — orders, line items, and payment records | `src/server/Modules/Sales` · `FluentPOS.Modules.Sales.*` |
| 3 | **People** | Customers and their in-progress shopping carts | `src/server/Modules/People` · `FluentPOS.Modules.People.*` |
| 4 | **Inventory** | Physical stock levels and the transactions that change them | `src/server/Modules/Inventory` · `FluentPOS.Modules.Inventory.*` |
| 5 | **Identity** | Authentication, authorisation, users, roles, and permissions | `src/server/Modules/Identity` · `FluentPOS.Modules.Identity.*` |
| 6 | **Accounting** | Financial reporting and ledger entries *(planned — not yet implemented)* | `src/server/Modules/Accounting` · `FluentPOS.Modules.Accounting.*` |

---

## 3. Glossary of Terms

### 3.1 Catalog

| Term | Definition | Aliases / Avoid | Code Reference |
|---|---|---|---|
| **Product** | A distinct item that the store sells, with its own price, tax rate, barcode, and category. The authoritative record of *what* is sold. | ~~item~~, ~~SKU~~, ~~goods~~ | `Modules.Catalog.Core/Entities/Product.cs` |
| **Brand** | The manufacturer or label associated with one or more Products (e.g., "Nike"). | ~~manufacturer~~, ~~vendor~~, ~~supplier~~ | `Modules.Catalog.Core/Entities/Brand.cs` |
| **Category** | A hierarchical classification for Products used for filtering and display (e.g., "Footwear"). | ~~group~~, ~~type~~, ~~folder~~ | `Modules.Catalog.Core/Entities/Category.cs` |
| **ProductImage** | The binary image data associated with a Product, retrieved separately from product metadata. | ~~photo~~, ~~thumbnail~~, ~~picture~~ | `GetProductImageQuery` · `Modules.Catalog.Core/Features/Products/Queries/` |
| **StockQuantity** / **AlertQuantity** | The threshold quantity below which a low-stock alert is triggered for a Product. Stored as `AlertQuantity` on `Product`. | ~~minimum stock~~, ~~reorder point~~ | `Product.AlertQuantity` · `Product.IsAlert` |
| **BarcodeSymbology** | The barcode format used to scan the Product at the point of sale (e.g., EAN-13, QR). | ~~barcode type~~, ~~scan format~~ | `Product.BarcodeSymbology` |
| **TaxMethod** | Whether tax is inclusive or exclusive in the Product's listed price. | ~~tax mode~~, ~~pricing method~~ | `Product.TaxMethod` |
| **ExtendedAttribute** (Catalog) | A dynamic key-value property attached to a Product, Brand, or Category when the fixed schema is insufficient. | ~~custom field~~, ~~metadata~~, ~~tag~~ | `Modules.Catalog.Core/Entities/ExtendedAttributes/` |

---

### 3.2 Sales

| Term | Definition | Aliases / Avoid | Code Reference |
|---|---|---|---|
| **Order** | A completed, immutable record of a sale — the customer, all purchased products, totals, and payment status. An Order is created when a Cart is checked out. | ~~sale~~, ~~invoice~~, ~~receipt~~, ~~transaction~~ | `Modules.Sales.Core/Entities/Order.cs` |
| **OrderItem** / **Product** (Sales) | A single line in an Order representing one Catalog Product, the quantity sold, its price at the time of sale, and the line-level tax and total. Named `Product` in the Sales entity model. | ~~line item~~, ~~order line~~, ~~cart line~~ | `Modules.Sales.Core/Entities/Product.cs` |
| **ReferenceNumber** | A human-readable, unique identifier assigned to an Order (e.g., `ORD-20240501-001`). Used in receipts and support queries. | ~~order number~~, ~~invoice number~~ | `Order.ReferenceNumber` |
| **Transaction** | A payment event against an Order, recording the payment type, amount tendered, and change due. One Order may have multiple Transactions (split payment). | ~~payment~~, ~~charge~~, ~~receipt~~ | `Modules.Sales.Core/Entities/Transaction.cs` |
| **PaymentType** | The method used to pay for an Order. Possible values: `Cash`, `CreditCard`, `GiftCard`. | ~~payment method~~, ~~tender type~~ | `Modules.Sales.Core/Enums/PaymentType.cs` |
| **Discount** | A monetary reduction applied to the Order's SubTotal before tax is computed. Stored as `Order.Discount`. | ~~markdown~~, ~~reduction~~ | `Order.Discount` |
| **SubTotal** | The sum of all OrderItem totals before Discount and Tax are applied. | ~~pre-tax total~~, ~~gross total~~ | `Order.SubTotal` |
| **Total** | The final amount the customer owes: `SubTotal − Discount + Tax`. | ~~grand total~~, ~~net payable~~ | `Order.Total` |
| **IsPaid** | Boolean flag on an Order indicating whether it has been fully settled by one or more Transactions. | ~~paid~~, ~~settled~~, ~~closed~~ | `Order.IsPaid` |

---

### 3.3 People

| Term | Definition | Aliases / Avoid | Code Reference |
|---|---|---|---|
| **Customer** | A person who buys from the store. Identified by name, phone, and email. Has zero or more Carts and appears on Orders. | ~~client~~, ~~buyer~~, ~~end user~~, ~~user~~ (reserved for Identity) | `Modules.People.Core/Entities/Customer.cs` |
| **Cart** | An in-progress, mutable collection of CartItems belonging to a Customer. A Cart becomes an Order when checked out. | ~~basket~~, ~~bag~~, ~~shopping cart~~ | `Modules.People.Core/Entities/Cart.cs` |
| **CartItem** | A single entry in a Cart representing a Catalog Product and the desired quantity. | ~~line~~, ~~cart line~~, ~~basket item~~ | `Modules.People.Core/Entities/CartItem.cs` |
| **Loyalty** / **LoyaltyPoints** | *(Planned)* A points balance earned by a Customer through purchases, redeemable against future Orders. Not yet implemented. | ~~rewards~~, ~~points~~ | — |
| **CustomerType** | A classification flag on a Customer (e.g., regular, VIP). Stored as `Customer.Type`. | ~~tier~~, ~~segment~~ | `Customer.Type` |
| **ExtendedAttribute** (People) | A dynamic key-value property attached to a Customer, Cart, or CartItem. | ~~custom field~~, ~~metadata~~ | `Modules.People.Core/Entities/ExtendedAttributes/` |

---

### 3.4 Inventory

| Term | Definition | Aliases / Avoid | Code Reference |
|---|---|---|---|
| **Stock** | The current available quantity of a specific Catalog Product in the warehouse. One Stock record per Product. | ~~inventory~~, ~~level~~, ~~on-hand~~ | `Modules.Inventory.Core/Entities/Stock.cs` |
| **StockTransaction** | An immutable record of a quantity change to Stock, driven by a Sale or a Purchase. Contains a `ReferenceNumber` linking it to the originating Order or purchase document. | ~~stock movement~~, ~~adjustment~~, ~~journal~~ | `Modules.Inventory.Core/Entities/StockTransaction.cs` |
| **TransactionType** | The direction of a StockTransaction. `Sale` decrements Stock; `Purchase` increments Stock. | ~~direction~~, ~~movement type~~ | `Modules.Inventory.Core/Enums/TransactionType.cs` |
| **AvailableQuantity** | The current count of units in Stock after all StockTransactions have been applied. | ~~quantity on hand~~, ~~balance~~ | `Stock.AvailableQuantity` |
| **Warehouse** | *(Planned)* A physical location where Stock is stored. Not yet modelled as a first-class entity. | ~~location~~, ~~store~~ | — |

---

### 3.5 Identity

| Term | Definition | Aliases / Avoid | Code Reference |
|---|---|---|---|
| **User** (FluentUser) | A person who operates the system — a cashier, manager, or admin. Distinct from Customer, who is served by the system. | ~~customer~~, ~~account~~, ~~member~~ | `Modules.Identity.Core/Entities/FluentUser.cs` |
| **Role** (FluentRole) | A named set of Permissions that can be assigned to one or more Users (e.g., `Superadmin`, `Staff`). | ~~group~~, ~~profile~~, ~~rank~~ | `Modules.Identity.Core/Entities/FluentRole.cs` |
| **Permission** | A fine-grained capability claim attached to a Role, controlling access to a specific API action (e.g., `Permissions.Products.Create`). | ~~right~~, ~~access~~, ~~scope~~ | `Modules.Identity.Core/Entities/FluentRoleClaim.cs` · `Shared.DTOs/Identity/Roles/PermissionResponse.cs` |
| **Token** (Access Token) | A short-lived JWT issued after successful authentication, authorising subsequent API calls. | ~~session~~, ~~credential~~, ~~auth token~~ | `FluentUser.RefreshToken` · `Shared.DTOs/Identity/Tokens/TokenResponse.cs` |
| **RefreshToken** | A long-lived secret stored on the User that can be exchanged for a new Access Token without re-entering credentials. | ~~session token~~, ~~re-auth token~~ | `FluentUser.RefreshToken` · `FluentUser.RefreshTokenExpiryTime` |
| **EventLog** | An immutable audit record of a significant system or domain event, used for security review and debugging. | ~~log entry~~, ~~audit trail~~, ~~history~~ | `Modules.Identity.Infrastructure/` · `Shared.DTOs/Identity/EventLogs/` |
| **ExtendedAttribute** (Identity) | A dynamic key-value property attached to a User or Role. | ~~custom field~~, ~~metadata~~ | `Modules.Identity.Core/Entities/ExtendedAttributes/` |

---

## 4. Domain Events

Domain Events are published via MediatR (`INotification`) and captured in the EventLog. Handlers in other modules may react without creating a direct dependency.

| Event | Bounded Context | When It Occurs |
|---|---|---|
| **ExtendedAttributeAddedEvent** | Shared | A new ExtendedAttribute key-value pair is attached to any entity |
| **ExtendedAttributeUpdatedEvent** | Shared | An existing ExtendedAttribute value is changed |
| **ExtendedAttributeRemovedEvent** | Shared | An ExtendedAttribute is detached from an entity |
| **ProductRegistered** *(implied by RegisterProductCommand)* | Catalog | A new Product is persisted for the first time |
| **ProductUpdated** *(implied by UpdateProductCommand)* | Catalog | An existing Product's data is changed |
| **ProductRemoved** *(implied by RemoveProductCommand)* | Catalog | A Product is deleted from the catalog |
| **CustomerRegistered** *(implied by RegisterCustomerCommand)* | People | A new Customer record is created |
| **SaleRegistered** *(implied by RegisterSaleCommand)* | Sales | An Order is placed and a Transaction recorded — triggers a StockTransaction in Inventory |
| **StockDecremented** *(implied by Sale)* | Inventory | Stock.AvailableQuantity is reduced after a completed Sale |
| **StockIncremented** *(implied by Purchase)* | Inventory | Stock.AvailableQuantity is increased after a Purchase StockTransaction |

> **Note:** Events marked *implied* are logically present but may not yet be published as explicit `INotification` classes. See `BaseEntity.AddDomainEvent()` in `Shared.Core/Domain/BaseEntity.cs` and `Shared.Core/Domain/Event.cs` for the event infrastructure.

---

## 5. Aggregates & Entities

An **Aggregate Root** is the only entry point for changes to the cluster of entities it owns. External code holds a reference only to the root, never to internal children.

### Catalog

| Aggregate Root | Child Entities / Value Objects | Key Invariants |
|---|---|---|
| **Product** | `ProductExtendedAttribute` (collection) | Price and Tax are non-negative; Brand and Category must exist |
| **Brand** | `BrandExtendedAttribute` (collection) | Name is unique |
| **Category** | `CategoryExtendedAttribute` (collection) | Name is unique |

### Sales

| Aggregate Root | Child Entities / Value Objects | Key Invariants |
|---|---|---|
| **Order** | `Product` (OrderItem, collection) | Total = SubTotal − Discount + Tax; immutable after creation |
| **Transaction** | — | Amount ≤ Order.Total; references a persisted Order |

### People

| Aggregate Root | Child Entities / Value Objects | Key Invariants |
|---|---|---|
| **Customer** | `CustomerExtendedAttribute` (collection) | Phone or Email must be present |
| **Cart** | `CartItem` (collection), `CustomerExtendedAttribute` ref | Belongs to exactly one Customer |
| **CartItem** | `CartItemExtendedAttribute` (collection) | Quantity > 0; references a valid Catalog Product |

### Inventory

| Aggregate Root | Child Entities / Value Objects | Key Invariants |
|---|---|---|
| **Stock** | — | One record per Product; AvailableQuantity ≥ 0 |
| **StockTransaction** | — | Immutable after creation; references a valid Product |

### Identity

| Aggregate Root | Child Entities / Value Objects | Key Invariants |
|---|---|---|
| **FluentUser** | `UserExtendedAttribute` (collection) | Email is unique (ASP.NET Core Identity constraint) |
| **FluentRole** | `FluentRoleClaim` (collection), `RoleExtendedAttribute` (collection) | Name is unique |
| **FluentRoleClaim** | — | Belongs to exactly one FluentRole |

---

## 6. Value Objects

A **Value Object** has no identity of its own — it is fully defined by its attributes and is immutable after creation.

| Value Object | Owning Aggregate | Description | Code Reference |
|---|---|---|---|
| **ExtendedAttribute** (generic) | Any entity | EAV-pattern dynamic property. Typed via `ExtendedAttributeType` (`Decimal`, `Text`, `DateTime`, `Json`, `Boolean`, `Integer`). | `Shared.Core/Domain/ExtendedAttribute.cs` |
| **ProductExtendedAttribute** | Product | Concrete ExtendedAttribute for Products | `Modules.Catalog.Core/Entities/ExtendedAttributes/ProductExtendedAttribute.cs` |
| **CategoryExtendedAttribute** | Category | Concrete ExtendedAttribute for Categories | `Modules.Catalog.Core/Entities/ExtendedAttributes/CategoryExtendedAttribute.cs` |
| **BrandExtendedAttribute** | Brand | Concrete ExtendedAttribute for Brands | `Modules.Catalog.Core/Entities/ExtendedAttributes/BrandExtendedAttribute.cs` |
| **CustomerExtendedAttribute** | Customer | Concrete ExtendedAttribute for Customers | `Modules.People.Core/Entities/ExtendedAttributes/CustomerExtendedAttribute.cs` |
| **CartExtendedAttribute** | Cart | Concrete ExtendedAttribute for Carts | `Modules.People.Core/Entities/ExtendedAttributes/CartExtendedAttribute.cs` |
| **CartItemExtendedAttribute** | CartItem | Concrete ExtendedAttribute for CartItems | `Modules.People.Core/Entities/ExtendedAttributes/CartItemExtendedAttribute.cs` |
| **UserExtendedAttribute** | FluentUser | Concrete ExtendedAttribute for Users | `Modules.Identity.Core/Entities/ExtendedAttributes/UserExtendedAttribute.cs` |
| **RoleExtendedAttribute** | FluentRole | Concrete ExtendedAttribute for Roles | `Modules.Identity.Core/Entities/ExtendedAttributes/RoleExtendedAttribute.cs` |
| **OrderItem** (Sales.Product) | Order | Snapshot of a Catalog Product at the moment of sale — price, tax, and quantity are frozen at checkout. | `Modules.Sales.Core/Entities/Product.cs` |
| **Money** *(conceptual)* | Order, Transaction | A monetary amount with an implicit currency (currently USD-implied). Not yet a first-class type — represented as `decimal`. | `Order.Total`, `Transaction.Amount` |
| **PaymentType** (enum) | Transaction | The tender method: `Cash`, `CreditCard`, `GiftCard`. | `Modules.Sales.Core/Enums/PaymentType.cs` |
| **TransactionType** (enum) | StockTransaction | The direction of a stock movement: `Sale` or `Purchase`. | `Modules.Inventory.Core/Enums/TransactionType.cs` |
| **ExtendedAttributeType** (enum) | ExtendedAttribute | The .NET type of the value stored in an ExtendedAttribute: `Decimal`, `Text`, `DateTime`, `Json`, `Boolean`, `Integer`. | `Shared.DTOs/ExtendedAttributes/ExtendedAttributeType.cs` |

---

## 7. Ubiquitous Language Rules

These rules are non-negotiable. Treat a violation the same as a failing test.

1. **Use domain terms everywhere.** Code symbols (class names, method names, variable names, database columns), test descriptions, API route segments, PR titles, issue titles, Slack messages, and comments must all use the terms defined in this document.

2. **Never use generic placeholders.** Forbidden words when a domain term exists: `data`, `info`, `stuff`, `record`, `object`, `item` (unless it is literally `CartItem`), `entity` (in business logic), `model` (outside of the persistence layer).

3. **Respect bounded-context boundaries.** Do not say "Product" when you mean an OrderItem in Sales. Do not say "User" when you mean a Customer in People. If the context is ambiguous, qualify it: `Catalog.Product`, `Sales.OrderItem`.

4. **Do not abbreviate domain terms.** Write `Customer`, not `Cust`. Write `Transaction`, not `Txn`. Abbreviations break searchability and confuse newcomers.

5. **Commands and queries follow the naming convention.** Commands are `<Verb><Noun>Command` (e.g., `RegisterProductCommand`, `RemoveCartItemCommand`). Queries are `Get<Noun>Query` or `Get<Noun>ByIdQuery`. Never name a command `Save`, `Process`, or `Handle`.

6. **An Order is immutable.** Once registered via `RegisterSaleCommand`, an Order must not be modified — issue a corrective Transaction or a refund flow instead. Never expose an `UpdateOrderCommand`.

7. **Cart ≠ Order.** A Cart is mutable and ephemeral; an Order is immutable and permanent. Do not use them interchangeably.

8. **User ≠ Customer.** A User logs into the back-office and operates the POS terminal. A Customer is served at the terminal. The same human may be both, but the concepts are separate and live in different bounded contexts.

9. **Keep this document current.** When a new entity, event, or value object is introduced, update this file in the same pull request. A term that exists in code but not in this glossary is a gap that must be closed.

10. **Resolve conflicts here.** If two team members use different terms for the same concept, open a PR against this file to settle it. The winning term must then be applied uniformly across the codebase before the PR is merged.

---

*FluentPOS — [https://github.com/fluentpos/fluentpos](https://github.com/fluentpos/fluentpos)*
