import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { cartApi } from '../api'
import type { Cart } from '../api/types'
import { useAuth } from '../auth/AuthContext'

interface CartContextValue {
  cart: Cart | null
  itemCount: number
  refresh: () => Promise<void>
  add: (productId: string, quantity: number) => Promise<void>
  update: (productId: string, quantity: number) => Promise<void>
  remove: (productId: string) => Promise<void>
  clear: () => Promise<void>
}

const CartContext = createContext<CartContextValue | null>(null)

export function CartProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  const [cart, setCart] = useState<Cart | null>(null)

  const refresh = useCallback(async () => {
    setCart(await cartApi.get())
  }, [])

  // Reload whenever a different user signs in; clear it on sign-out
  const userId = user?.userId
  useEffect(() => {
    if (!userId) {
      setCart(null)
      return
    }
    refresh().catch(() => setCart(null))
  }, [userId, refresh])

  const value = useMemo<CartContextValue>(
    () => ({
      cart,
      itemCount: cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0,
      refresh,
      add: async (productId, quantity) => setCart(await cartApi.add(productId, quantity)),
      update: async (productId, quantity) => setCart(await cartApi.update(productId, quantity)),
      remove: async (productId) => {
        await cartApi.remove(productId)
        await refresh()
      },
      clear: async () => {
        await cartApi.clear()
        await refresh()
      },
    }),
    [cart, refresh],
  )

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}

export function useCart() {
  const ctx = useContext(CartContext)
  if (!ctx) throw new Error('useCart must be used inside CartProvider')
  return ctx
}
