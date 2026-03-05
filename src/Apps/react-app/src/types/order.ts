import type { MoneyDto } from './cart'

// ===== Enums =====

export type OrderStatus =
    | 'Draft'
    | 'Pending'
    | 'Reserved'
    | 'Confirmed'
    | 'Shipped'
    | 'Delivered'
    | 'Cancelled'
    | 'Refunded'

export type PaymentMethod = 'Online' | 'COD'

export type PaymentStatus = 'Pending' | 'Paid' | 'Failed' | 'Refunded'

export type ShippingMethod = 'Standard' | 'Fast' | 'SuperFast'

export type ShipmentStatus =
    | 'Created'
    | 'AwaitingPickup'
    | 'PickedUp'
    | 'InTransit'
    | 'OutForDelivery'
    | 'Delivered'
    | 'DeliveryFailed'
    | 'Returned'
    | 'Cancelled'

export type TrackingEventType =
    | 'Created'
    | 'PickupScheduled'
    | 'PickedUp'
    | 'ArrivedAtHub'
    | 'InTransit'
    | 'OutForDelivery'
    | 'DeliveryAttempted'
    | 'Delivered'
    | 'DeliveryFailed'
    | 'Returning'
    | 'Returned'
    | 'Cancelled'
    | 'Other'

export type DiscountSourceType = 'Coupon' | 'Promotion' | 'Manual'

export type PaymentProvider = 'VNPay' | 'Momo' | 'ZaloPay' | 'COD' | 'BankTransfer'

// ===== DTOs =====

export type AddressDto = {
    FullName: string
    Phone: string
    Street: string
    Ward: string
    District: string
    City: string
    Note?: string | null
}

export type OrderItemDto = {
    Id: string
    ProductId: string
    VariantId: string
    Sku: string
    ProductName: string
    VariantName: string
    Thumbnail?: string | null
    UnitPrice: MoneyDto
    Quantity: number
    DiscountAmount: MoneyDto
    LineTotal: MoneyDto
    LineTotalAfterDiscount: MoneyDto
}

export type OrderDiscountDto = {
    Id: string
    Source: DiscountSourceType
    SourceId?: string | null
    SourceCode?: string | null
    Description: string
    Amount: MoneyDto
}

export type OrderStatusHistoryDto = {
    Id: string
    FromStatus: OrderStatus
    ToStatus: OrderStatus
    Reason?: string | null
    ChangedBy?: string | null
    CreatedAt: string
}

export type TrackingEventDto = {
    Id: string
    EventType: TrackingEventType
    Description: string
    Location?: string | null
    Timestamp: string
}

export type ShippingInfoDto = {
    ShipmentNumber?: string | null
    Provider?: string | null
    TrackingNumber?: string | null
    Status: ShipmentStatus
    EstimatedDeliveryDate?: string | null
    ActualDeliveryDate?: string | null
    TrackingEvents: TrackingEventDto[]
}

export type OrderDto = {
    Id: string
    OrderNumber: string
    CustomerId: string
    Status: OrderStatus
    ShippingAddress: AddressDto
    BillingAddress?: AddressDto | null
    PaymentMethod: PaymentMethod
    PaymentStatus: PaymentStatus
    PaidAt?: string | null
    ShippingMethod: ShippingMethod
    ShippingInfo?: ShippingInfoDto | null
    SubTotal: MoneyDto
    ShippingFee: MoneyDto
    TotalDiscount: MoneyDto
    GrandTotal: MoneyDto
    CustomerNote?: string | null
    CancellationReason?: string | null
    Items: OrderItemDto[]
    Discounts: OrderDiscountDto[]
    StatusHistory: OrderStatusHistoryDto[]
    CreatedAt: string
    UpdatedAt: string
}

export type OrderSummaryDto = {
    Id: string
    OrderNumber: string
    Status: OrderStatus
    PaymentMethod: PaymentMethod
    PaymentStatus: PaymentStatus
    ShippingMethod: ShippingMethod
    ItemCount: number
    FirstItemThumbnail?: string | null
    FirstItemName: string
    GrandTotal: MoneyDto
    CreatedAt: string
}

// ===== Requests =====

export type PlaceOrderRequest = {
    ShippingAddress: AddressDto
    PaymentMethod: PaymentMethod
    ShippingMethod: ShippingMethod
    VoucherCode?: string | null
    CustomerNote?: string | null
}

export type CancelOrderRequest = {
    Reason: string
}

// ===== Responses =====

export type ApplyVoucherResponse = {
    Success: boolean
    CouponCode?: string | null
    Description?: string | null
    DiscountAmount?: MoneyDto | null
    ErrorMessage?: string | null
}

export type ActivePromotionDto = {
    Id: string
    Name: string
    Description?: string | null
    Type: string
    ActionDescription: string
    EndDate: string
}

export type ShippingFeeDto = {
    Method: ShippingMethod
    Label: string
    Description: string
    Fee: MoneyDto
    EstimatedDays: string
}

export type UserVoucherDto = {
    Code: string
    Description: string
    DiscountType: 'Percentage' | 'FixedAmount'
    DiscountValue: number
    MinOrderAmount: number
    MaxDiscount?: number
    ExpiresAt: string
}
