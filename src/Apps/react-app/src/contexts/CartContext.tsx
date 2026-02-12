import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import axios from 'axios'
import { useAuth } from '../hooks/useAuth'
import * as cartApi from '../services/cartApi'
import type { CartDto, CartSummaryDto } from '../types/cart'

type CartContextValue = {
  cart: CartDto | null
  summary: CartSummaryDto | null
  isLoading: boolean
  isMutating: boolean
  error: string | null
  refreshCart: () => Promise<void>
  addItem: (sku: string, quantity?: number) => Promise<void>
  updateQuantity: (variantId: string, quantity: number) => Promise<void>
  removeItem: (variantId: string) => Promise<void>
  clear: () => Promise<void>
}

export const CartContext = createContext<CartContextValue | undefined>(undefined)

const getErrorMessage = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const payload = error.response?.data as { message?: string; title?: string } | string | undefined
    if (typeof payload === 'string') {
      return payload
    }

    if (payload?.message) {
      return payload.message
    }

    if (payload?.title) {
      return payload.title
    }
  }

  return 'Không thể xử lý giỏ hàng. Vui lòng thử lại.'
}

function toSummary(cart: CartDto | null): CartSummaryDto | null {
  return cart?.Summary ?? null
}

export function CartProvider({ children }: { children: ReactNode }) {
  const { isInitializing, isAuthenticated } = useAuth()
  const [cart, setCart] = useState<CartDto | null>(null)
  const [summary, setSummary] = useState<CartSummaryDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isMutating, setIsMutating] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const applyCart = useCallback((nextCart: CartDto | null) => {
    setCart(nextCart)
    setSummary(toSummary(nextCart))
  }, [])

  const refreshCart = useCallback(async () => {
    setError(null)
    const currentCart = await cartApi.getMyCart()
    applyCart(currentCart)
  }, [applyCart])

  useEffect(() => {
    if (isInitializing) {
      return
    }

    let isMounted = true
    setIsLoading(true)
    setError(null)

    void (async () => {
      try {
        const currentCart = await cartApi.getMyCart()
        if (!isMounted) return
        applyCart(currentCart)
      } catch (fetchError) {
        if (!isMounted) return
        setError(getErrorMessage(fetchError))
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    })()

    return () => {
      isMounted = false
    }
  }, [applyCart, isAuthenticated, isInitializing])

  useEffect(() => {
    const onRefreshRequested = () => {
      void refreshCart()
    }

    window.addEventListener('cart:refresh-requested', onRefreshRequested)
    return () => {
      window.removeEventListener('cart:refresh-requested', onRefreshRequested)
    }
  }, [refreshCart])

  const runMutating = useCallback(async (action: () => Promise<CartDto | null>) => {
    setIsMutating(true)
    setError(null)
    try {
      const nextCart = await action()
      applyCart(nextCart)
    } catch (mutationError) {
      setError(getErrorMessage(mutationError))
      throw mutationError
    } finally {
      setIsMutating(false)
    }
  }, [applyCart])

  const addItem = useCallback(async (sku: string, quantity = 1) => {
    await runMutating(() => cartApi.addCartItem({ sku, quantity }))
  }, [runMutating])

  const updateQuantity = useCallback(async (variantId: string, quantity: number) => {
    await runMutating(() => cartApi.updateCartItemQuantity(variantId, { quantity }))
  }, [runMutating])

  const removeItem = useCallback(async (variantId: string) => {
    await runMutating(() => cartApi.removeCartItem(variantId))
  }, [runMutating])

  const clear = useCallback(async () => {
    await runMutating(() => cartApi.clearCart())
  }, [runMutating])

  const value = useMemo<CartContextValue>(() => ({
    cart,
    summary,
    isLoading,
    isMutating,
    error,
    refreshCart,
    addItem,
    updateQuantity,
    removeItem,
    clear,
  }), [addItem, cart, clear, error, isLoading, isMutating, refreshCart, removeItem, summary, updateQuantity])

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}
