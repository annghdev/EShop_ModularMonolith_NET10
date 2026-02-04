import { type ReactNode, useEffect, useMemo, useState } from 'react'
import * as Dialog from '@radix-ui/react-dialog'

type CartItem = {
  id: string
  name: string
  price: number
  quantity: number
  image: string
}

type CartDrawerProps = {
  trigger: ReactNode
  onSelectedCountChange?: (count: number) => void
}

const CART_ITEMS: CartItem[] = [
  {
    id: 'cart-1',
    name: 'Premium T-Shirt 16',
    price: 237200,
    quantity: 1,
    image: 'https://picsum.photos/seed/shirt16/120/120.jpg',
  },
  {
    id: 'cart-2',
    name: 'DELL Laptop Pro 18',
    price: 27292186,
    quantity: 1,
    image: 'https://picsum.photos/seed/laptop18/120/120.jpg',
  },
  {
    id: 'cart-3',
    name: 'Nebula Pods',
    price: 2950000,
    quantity: 2,
    image: 'https://picsum.photos/seed/audio3/120/120.jpg',
  },
]

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
  const [selectedIds, setSelectedIds] = useState<Set<string>>(
    () => new Set(CART_ITEMS.map((item) => item.id)),
  )
  const [quantities, setQuantities] = useState<Record<string, number>>(
    () => Object.fromEntries(CART_ITEMS.map((item) => [item.id, item.quantity])),
  )

  const selectedCount = selectedIds.size
  const allSelected = selectedCount === CART_ITEMS.length

  const totalAmount = useMemo(() => {
    return CART_ITEMS.reduce((total, item) => {
      if (!selectedIds.has(item.id)) return total
      const qty = quantities[item.id] ?? item.quantity
      return total + item.price * qty
    }, 0)
  }, [quantities, selectedIds])

  const updateQuantity = (id: string, delta: number) => {
    setQuantities((prev) => {
      const next = { ...prev }
      const current = next[id] ?? 1
      next[id] = Math.max(1, current + delta)
      return next
    })
  }

  useEffect(() => {
    onSelectedCountChange?.(selectedCount)
  }, [onSelectedCountChange, selectedCount])

  const toggleSelectAll = () => {
    if (allSelected) {
      setSelectedIds(new Set())
      return
    }
    setSelectedIds(new Set(CART_ITEMS.map((item) => item.id)))
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
    <Dialog.Root>
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
            {CART_ITEMS.map((item) => (
              <div className="cart-item" key={item.id}>
                <img src={item.image} alt={item.name} />
                <div className="cart-item-info">
                  <h4>{item.name}</h4>
                  <p>{formatCurrency(item.price)}</p>
                  <div className="cart-qty">
                    <span>Số lượng</span>
                    <div className="qty-controls">
                      <button
                        type="button"
                        aria-label="Giảm số lượng"
                        onClick={() => updateQuantity(item.id, -1)}
                      >
                        −
                      </button>
                      <strong>{quantities[item.id] ?? item.quantity}</strong>
                      <button
                        type="button"
                        aria-label="Tăng số lượng"
                        onClick={() => updateQuantity(item.id, 1)}
                      >
                        +
                      </button>
                    </div>
                  </div>
                </div>
                <label className="checkbox-line item-check">
                  <input
                    type="checkbox"
                    checked={selectedIds.has(item.id)}
                    onChange={() => toggleItem(item.id)}
                  />
                </label>
                <button type="button" className="cart-item-remove" aria-label="Xóa sản phẩm">
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
            <button className="btn btn-primary" type="button">
              Thanh toán
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}

export default CartDrawer
