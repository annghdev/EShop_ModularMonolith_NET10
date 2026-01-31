# Module ERDs

Entity-Relationship diagrams below reflect each module's domain (aggregates and entities). References to IDs from other modules (e.g. `ProductId`, `OrderId`) are logical; each module uses its own database.

## Catalog (CatalogDb)

Product catalog: categories (tree), attributes/values, brands, products with variants and product attributes, collections.

```mermaid
erDiagram
    Category ||--o{ Category : "parent-child"
    Category ||--o{ CategoryDefaultAttribute : "has"
    Attribute ||--o{ AttributeValue : "has"
    CategoryDefaultAttribute }o--|| Attribute : "references"
    CategoryDefaultAttribute }o--|| Category : "belongs to"
    Product }o--|| Category : "CategoryId"
    Product }o--|| Brand : "BrandId"
    Product ||--o{ ProductAttribute : "has"
    Product ||--o{ Variant : "has"
    ProductAttribute }o--|| Attribute : "AttributeId"
    Variant }o--o{ VariantAttributeValue : "has"
    VariantAttributeValue }o--|| ProductAttribute : "ProductAttributeId"
    VariantAttributeValue }o--|| AttributeValue : "ValueId"
    Collection ||--o{ CollectionItem : "has"
    CollectionItem }o--|| Product : "ProductId"
    Brand {
        guid Id PK
        string Name
        string Logo
    }
    Category {
        guid Id PK
        string Name
        guid ParentId FK
    }
    Attribute {
        guid Id PK
        string Name
    }
    AttributeValue {
        guid Id PK
        guid AttributeId FK
        string Name
    }
    Product {
        guid Id PK
        guid CategoryId FK
        guid BrandId FK
        string Name
        string Status
    }
    ProductAttribute {
        guid Id PK
        guid ProductId FK
        guid AttributeId FK
        int DisplayOrder
        bool HasVariant
    }
    Variant {
        guid Id PK
        guid ProductId FK
        string Name
        string Sku
    }
    VariantAttributeValue {
        guid Id PK
        guid VariantId FK
        guid ProductAttributeId FK
        guid ValueId FK
    }
    Collection {
        guid Id PK
        string Name
    }
    CollectionItem {
        guid Id PK
        guid CollectionId FK
        guid ProductId FK
    }
```

## Inventory (InventoryDb)

Warehouses and inventory items per variant; reservations for orders; movement log.

```mermaid
erDiagram
    Warehouse ||--o{ InventoryItem : "contains"
    InventoryItem ||--o{ InventoryReservation : "has"
    InventoryItem ||--o{ InventoryMovement : "has"
    Warehouse {
        guid Id PK
        string Code
        string Name
        bool IsActive
    }
    InventoryItem {
        guid Id PK
        guid WarehouseId FK
        guid ProductId "ref Catalog"
        guid VariantId "ref Catalog"
        string Sku
        int QuantityOnHand
        int LowStockThreshold
    }
    InventoryReservation {
        guid Id PK
        guid InventoryItemId FK
        guid OrderId "ref Orders"
        int Quantity
    }
    InventoryMovement {
        guid Id PK
        guid InventoryItemId FK
        guid OrderId "ref Orders"
        int Quantity
        string Type
    }
```

## Orders (OrderDb)

Orders with items, discounts (coupon/promotion), and status history.

```mermaid
erDiagram
    Order ||--o{ OrderItem : "has"
    Order ||--o{ OrderDiscount : "has"
    Order ||--o{ OrderStatusHistory : "has"
    Order {
        guid Id PK
        string OrderNumber
        guid CustomerId "ref Users"
        guid PaymentId "ref Payment"
        guid ShippingId "ref Shipping"
        string PaymentMethod
        string ShippingMethod
        string Status
        money SubTotal
        money GrandTotal
    }
    OrderItem {
        guid Id PK
        guid OrderId FK
        guid ProductId "ref Catalog"
        guid VariantId "ref Catalog"
        string Sku
        int Quantity
        money UnitPrice
    }
    OrderDiscount {
        guid Id PK
        guid OrderId FK
        string Source
        guid SourceId "Coupon/Promotion"
        money Amount
    }
    OrderStatusHistory {
        guid Id PK
        guid OrderId FK
        string FromStatus
        string ToStatus
    }
```

## Payment (PaymentDb)

Payment gateways with settings; transactions and refunds linked to orders.

```mermaid
erDiagram
    PaymentGateway ||--o| PaymentGatewaySetting : "has"
    PaymentTransaction ||--o{ PaymentRefund : "has"
    PaymentTransaction }o--o| PaymentGateway : "GatewayId"
    PaymentGateway {
        guid Id PK
        string Name
        string Provider
        bool IsEnabled
    }
    PaymentGatewaySetting {
        guid Id PK
        guid PaymentGatewayId FK
        string ApiKey
        string SecretKey
    }
    PaymentTransaction {
        guid Id PK
        string TransactionNumber
        guid OrderId "ref Orders"
        guid GatewayId FK
        guid CustomerId "ref Users"
        money Amount
        string Status
    }
    PaymentRefund {
        guid Id PK
        guid PaymentTransactionId FK
        money RefundAmount
        string Status
    }
```

## Pricing (PricingDb)

Product/variant prices with change log; coupons with conditions and usages; promotions with rules and actions.

```mermaid
erDiagram
    ProductPrice ||--o{ PriceChangeLog : "has"
    Coupon ||--o{ CouponCondition : "has"
    Coupon ||--o{ CouponUsage : "has"
    Promotion ||--o{ PromotionRule : "has"
    Promotion ||--o{ PromotionAction : "has"
    ProductPrice {
        guid Id PK
        guid ProductId "ref Catalog"
        guid VariantId "ref Catalog"
        string Sku
        money CurrentCost
        money CurrentPrice
    }
    PriceChangeLog {
        guid Id PK
        guid ProductPriceId FK
        money PreviousPrice
        money NewPrice
    }
    Coupon {
        guid Id PK
        string Code
        string Name
        money MinOrderValue
        datetime StartDate
        datetime EndDate
    }
    CouponCondition {
        guid Id PK
        guid CouponId FK
        string Type
        guid TargetId
    }
    CouponUsage {
        guid Id PK
        guid CouponId FK
        guid CustomerId "ref Users"
        guid OrderId "ref Orders"
        money DiscountApplied
    }
    Promotion {
        guid Id PK
        string Name
        string Type
        datetime StartDate
        datetime EndDate
    }
    PromotionRule {
        guid Id PK
        guid PromotionId FK
        string Type
        guid TargetId
    }
    PromotionAction {
        guid Id PK
        guid PromotionId FK
        string Type
        string Discount
    }
```

## Shipping (ShippingDb)

Carriers with settings; shipments per order with items and tracking events.

```mermaid
erDiagram
    ShippingCarrier ||--o| ShippingCarrierSetting : "has"
    Shipment }o--o| ShippingCarrier : "CarrierId"
    Shipment ||--o{ ShipmentItem : "has"
    Shipment ||--o{ TrackingEvent : "has"
    ShippingCarrier {
        guid Id PK
        string Name
        string Provider
        bool IsEnabled
    }
    ShippingCarrierSetting {
        guid Id PK
        guid ShippingCarrierId FK
        string ApiToken
    }
    Shipment {
        guid Id PK
        string ShipmentNumber
        guid OrderId "ref Orders"
        guid CarrierId FK
        string TrackingNumber
        string Status
    }
    ShipmentItem {
        guid Id PK
        guid ShipmentId FK
        guid VariantId "ref Catalog"
        string Sku
        int Quantity
    }
    TrackingEvent {
        guid Id PK
        guid ShipmentId FK
        string EventType
        string Description
        datetime Timestamp
    }
```

## ShoppingCart (ShoppingCartDb)

Carts (customer or guest) and cart items; optional applied coupon.

```mermaid
erDiagram
    Cart ||--o{ CartItem : "has"
    Cart {
        guid Id PK
        string CustomerId "or GuestId"
        string Status
        string AppliedCouponCode
        guid AppliedCouponId "ref Pricing"
        money SubTotal
        money EstimatedTotal
    }
    CartItem {
        guid Id PK
        guid CartId FK
        guid ProductId "ref Catalog"
        guid VariantId "ref Catalog"
        string Sku
        int Quantity
        money UnitPrice
    }
```

## Users (UsersDb)

Customers with addresses; employees; guests (anonymous). References to Auth (AccountId) are logical.

```mermaid
erDiagram
    Customer ||--o{ CustomerAddress : "has"
    Customer {
        guid Id PK
        string FullName
        string Email
        guid AccountId "ref Auth"
        string Tier
        int LoyaltyPoints
    }
    CustomerAddress {
        guid Id PK
        guid CustomerId FK
        string Address
        bool IsDefault
    }
    Employee {
        guid Id PK
        string EmployeeCode
        string FullName
        guid AccountId "ref Auth"
        string Status
    }
    Guest {
        guid Id PK
        string ClientId
        string Email
        guid ConvertedToCustomerId "ref Customer"
    }
```
