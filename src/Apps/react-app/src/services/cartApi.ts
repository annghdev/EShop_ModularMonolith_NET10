import api, { API_PREFIX } from '../config/api'
import type { CartDto, CartSummaryDto } from '../types/cart'

export type AddCartItemRequest = {
  sku: string
  quantity?: number
}

export type UpdateCartItemQuantityRequest = {
  quantity: number
}

const cartBase = `${API_PREFIX}/cart`

type UnknownRecord = Record<string, unknown>

function getValue<T>(source: UnknownRecord, pascalKey: string, camelKey: string, fallback: T): T {
  const value = source[pascalKey] ?? source[camelKey]
  return (value as T) ?? fallback
}

function normalizeMoney(raw: unknown) {
  const source = (raw ?? {}) as UnknownRecord
  return {
    Amount: Number(getValue(source, 'Amount', 'amount', 0)),
    Currency: getValue<string | null>(source, 'Currency', 'currency', null),
  }
}

function normalizeCartSummary(raw: unknown): CartSummaryDto {
  const source = (raw ?? {}) as UnknownRecord
  return {
    ItemCount: Number(getValue(source, 'ItemCount', 'itemCount', 0)),
    TotalQuantity: Number(getValue(source, 'TotalQuantity', 'totalQuantity', 0)),
    SubTotal: normalizeMoney(getValue(source, 'SubTotal', 'subTotal', null)),
    TotalDiscount: normalizeMoney(getValue(source, 'TotalDiscount', 'totalDiscount', null)),
    EstimatedTotal: normalizeMoney(getValue(source, 'EstimatedTotal', 'estimatedTotal', null)),
  }
}

function normalizeCart(raw: unknown): CartDto | null {
  if (!raw) {
    return null
  }

  const source = raw as UnknownRecord
  const rawItems = getValue<unknown[]>(source, 'Items', 'items', [])

  return {
    CartId: String(getValue(source, 'CartId', 'cartId', '')),
    OwnerType: String(getValue(source, 'OwnerType', 'ownerType', '')),
    CustomerId: getValue<string | null>(source, 'CustomerId', 'customerId', null),
    GuestId: getValue<string | null>(source, 'GuestId', 'guestId', null),
    Status: String(getValue(source, 'Status', 'status', '')),
    AppliedCouponCode: getValue<string | null>(source, 'AppliedCouponCode', 'appliedCouponCode', null),
    LastActivityAt: getValue<string | null>(source, 'LastActivityAt', 'lastActivityAt', null),
    Items: rawItems.map((item) => {
      const itemSource = (item ?? {}) as UnknownRecord
      return {
        ItemId: String(getValue(itemSource, 'ItemId', 'itemId', '')),
        ProductId: String(getValue(itemSource, 'ProductId', 'productId', '')),
        VariantId: String(getValue(itemSource, 'VariantId', 'variantId', '')),
        Sku: String(getValue(itemSource, 'Sku', 'sku', '')),
        ProductName: String(getValue(itemSource, 'ProductName', 'productName', '')),
        VariantName: String(getValue(itemSource, 'VariantName', 'variantName', '')),
        Thumbnail: getValue<string | null>(itemSource, 'Thumbnail', 'thumbnail', null),
        Quantity: Number(getValue(itemSource, 'Quantity', 'quantity', 0)),
        OriginalPrice: normalizeMoney(getValue(itemSource, 'OriginalPrice', 'originalPrice', null)),
        UnitPrice: normalizeMoney(getValue(itemSource, 'UnitPrice', 'unitPrice', null)),
        DiscountAmount: normalizeMoney(getValue(itemSource, 'DiscountAmount', 'discountAmount', null)),
        LineTotal: normalizeMoney(getValue(itemSource, 'LineTotal', 'lineTotal', null)),
        LineTotalDiscount: normalizeMoney(getValue(itemSource, 'LineTotalDiscount', 'lineTotalDiscount', null)),
      }
    }),
    Summary: normalizeCartSummary(getValue(source, 'Summary', 'summary', null)),
  }
}

export async function getMyCart(): Promise<CartDto | null> {
  const response = await api.get<unknown>(cartBase)
  return normalizeCart(response.data)
}

export async function getCartSummary(): Promise<CartSummaryDto> {
  const response = await api.get<unknown>(`${cartBase}/count`)
  return normalizeCartSummary(response.data)
}

export async function addCartItem(request: AddCartItemRequest): Promise<CartDto> {
  const response = await api.post<unknown>(`${cartBase}/items`, request)
  return normalizeCart(response.data) as CartDto
}

export async function updateCartItemQuantity(variantId: string, request: UpdateCartItemQuantityRequest): Promise<CartDto> {
  const response = await api.put<unknown>(`${cartBase}/items/${variantId}`, request)
  return normalizeCart(response.data) as CartDto
}

export async function removeCartItem(variantId: string): Promise<CartDto> {
  const response = await api.delete<unknown>(`${cartBase}/items/${variantId}`)
  return normalizeCart(response.data) as CartDto
}

export async function clearCart(): Promise<CartDto | null> {
  const response = await api.delete<unknown>(`${cartBase}/items`)
  return normalizeCart(response.data)
}

export async function mergeGuestCart(guestId: string): Promise<CartDto> {
  const response = await api.post<unknown>(`${cartBase}/merge-guest`, { guestId })
  return normalizeCart(response.data) as CartDto
}
