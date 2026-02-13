import { type ReactNode, useEffect, useMemo, useState } from 'react'
import * as Dialog from '@radix-ui/react-dialog'
import { useNavigate } from 'react-router'
import { useCart } from '../hooks/useCart'
import type { CartItemDto } from '../types/cart'

type CartDrawerProps = {
  trigger: ReactNode
  onSelectedCountChange?: (count: number) => void
}

const EMPTY_ITEMS: CartItemDto[] = []

function areSetsEqual(left: Set<string>, right: Set<string>) {
  if (left.size !== right.size) {
    return false
  }

  for (const value of left) {
    if (!right.has(value)) {
      return false
    }
  }

  return true
}

const formatCurrency = (value: number) => {
  try {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(value)
  } catch {
    return `${value.toLocaleString('vi-VN')} đ`
  }
}

function CartDrawer({ trigger, onSelectedCountChange }: CartDrawerProps) {
  const navigate = useNavigate()
  const { cart, isLoading, isMutating, updateQuantity, removeItem } = useCart()
  const items = useMemo(() => cart?.Items ?? EMPTY_ITEMS, [cart?.Items])
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [open, setOpen] = useState(false)

  const handleCheckout = () => {
    setOpen(false)
    navigate('/order')
  }

  useEffect(() => {
    setSelectedIds((prev) => {
      const next = new Set<string>()
      const itemIds = new Set(items.map((item) => item.VariantId))

      prev.forEach((id) => {
        if (itemIds.has(id)) {
          next.add(id)
        }
      })

      items.forEach((item) => {
        if (!prev.has(item.VariantId)) {
          next.add(item.VariantId)
        }
      })

      return areSetsEqual(prev, next) ? prev : next
    })
  }, [items])

  const selectedCount = useMemo(() => {
    return items.reduce((count, item) => count + (selectedIds.has(item.VariantId) ? 1 : 0), 0)
  }, [items, selectedIds])

  const allSelected = items.length > 0 && selectedCount === items.length

  const totalAmount = useMemo(() => {
    return items.reduce((total, item) => {
      if (!selectedIds.has(item.VariantId)) return total
      return total + (item.LineTotal?.Amount ?? 0)
    }, 0)
  }, [items, selectedIds])

  const handleUpdateQuantity = async (variantId: string, currentQuantity: number, delta: number) => {
    const nextQuantity = Math.max(1, currentQuantity + delta)
    await updateQuantity(variantId, nextQuantity)
  }

  const handleRemoveItem = async (variantId: string) => {
    await removeItem(variantId)
  }

  useEffect(() => {
    onSelectedCountChange?.(selectedCount)
  }, [onSelectedCountChange, selectedCount])

  const toggleSelectAll = () => {
    if (allSelected) {
      setSelectedIds(new Set())
      return
    }
    setSelectedIds(new Set(items.map((item) => item.VariantId)))
  }

  const toggleItem = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) {
        next.delete(id)
      } else {
        next.add(id)
      }
      return next
    })
  }

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger asChild>{trigger}</Dialog.Trigger>
      <Dialog.Portal>
        <Dialog.Overlay className="radix-overlay" />
        <Dialog.Content className="radix-drawer">
          <div className="drawer-header">
            <div>
              <Dialog.Title className="drawer-title">Giỏ hàng</Dialog.Title>
              <Dialog.Description className="drawer-subtitle">
                {selectedCount} sản phẩm được chọn
              </Dialog.Description>
            </div>
            <Dialog.Close className="icon-button" aria-label="Đóng giỏ hàng">
              ✕
            </Dialog.Close>
          </div>

          <div className="drawer-select-all">
            <label className="checkbox-line">
              <input
                type="checkbox"
                checked={allSelected}
                onChange={toggleSelectAll}
              />
              <span>Chọn tất cả</span>
            </label>
          </div>

          <div className="drawer-items">
            {isLoading && (
              <div className="empty-state">Đang tải giỏ hàng...</div>
            )}
            {!isLoading && items.length === 0 && (
              <div className="empty-state">Giỏ hàng của bạn đang trống.</div>
            )}
            {!isLoading && items.map((item) => (
              <div className="cart-item" key={item.ItemId}>
                <img src={item.Thumbnail || 'https://picsum.photos/seed/cart-empty/120/120.jpg'} alt={item.ProductName} />
                <div className="cart-item-info">
                  <h4>{item.ProductName} - {item.VariantName}</h4>
                  <p>{formatCurrency(item.UnitPrice?.Amount ?? 0)}</p>
                  <div className="cart-qty">
                    <span>Số lượng</span>
                    <div className="qty-controls">
                      <button
                        type="button"
                        aria-label="Giảm số lượng"
                        disabled={isMutating}
                        onClick={() => { void handleUpdateQuantity(item.VariantId, item.Quantity, -1) }}
                      >
                        −
                      </button>
                      <strong>{item.Quantity}</strong>
                      <button
                        type="button"
                        aria-label="Tăng số lượng"
                        disabled={isMutating}
                        onClick={() => { void handleUpdateQuantity(item.VariantId, item.Quantity, 1) }}
                      >
                        +
                      </button>
                    </div>
                  </div>
                </div>
                <label className="checkbox-line item-check">
                  <input
                    type="checkbox"
                    checked={selectedIds.has(item.VariantId)}
                    onChange={() => toggleItem(item.VariantId)}
                  />
                </label>
                <button
                  type="button"
                  className="cart-item-remove"
                  aria-label="Xóa sản phẩm"
                  disabled={isMutating}
                  onClick={() => { void handleRemoveItem(item.VariantId) }}
                >
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18" /><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6" /><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2" /><line x1="10" x2="10" y1="11" y2="17" /><line x1="14" x2="14" y1="11" y2="17" /></svg>
                </button>
              </div>
            ))}
          </div>

          <div className="drawer-footer">
            <div className="drawer-total">
              <span>Tạm tính</span>
              <strong>{formatCurrency(totalAmount)}</strong>
            </div>
            <button className="btn btn-primary" type="button" onClick={handleCheckout}>
              Thanh toán
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}

export default CartDrawer
