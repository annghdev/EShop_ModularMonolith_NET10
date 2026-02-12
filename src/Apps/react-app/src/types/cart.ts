export type MoneyDto = {
  Amount: number
  Currency?: string | null
}

export type CartItemDto = {
  ItemId: string
  ProductId: string
  VariantId: string
  Sku: string
  ProductName: string
  VariantName: string
  Thumbnail?: string | null
  Quantity: number
  OriginalPrice: MoneyDto
  UnitPrice: MoneyDto
  DiscountAmount: MoneyDto
  LineTotal: MoneyDto
  LineTotalDiscount: MoneyDto
}

export type CartSummaryDto = {
  ItemCount: number
  TotalQuantity: number
  SubTotal: MoneyDto
  TotalDiscount: MoneyDto
  EstimatedTotal: MoneyDto
}

export type CartDto = {
  CartId: string
  OwnerType: string
  CustomerId?: string | null
  GuestId?: string | null
  Status: string
  AppliedCouponCode?: string | null
  LastActivityAt?: string | null
  Items: CartItemDto[]
  Summary: CartSummaryDto
}
