import { useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import type { Product } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { useCart } from '../cart/CartContext'
import { useToast } from './Toast'
import { errorMessage } from './ui'

export function AddToCart({ product, quantity = 1 }: { product: Product; quantity?: number }) {
  const { user } = useAuth()
  const { cart, add } = useCart()
  const toast = useToast()
  const location = useLocation()
  const [busy, setBusy] = useState(false)

  if (!user) {
    return (
      <Link to="/login" state={{ from: location.pathname + location.search }} className="btn btn-secondary">
        Sign in to buy
      </Link>
    )
  }

  const inCart = cart?.items.find((i) => i.productId === product.id)?.quantity ?? 0
  const soldOut = product.stockQuantity === 0
  const atLimit = inCart + quantity > product.stockQuantity

  return (
    <button
      className="btn btn-primary"
      disabled={busy || soldOut || atLimit}
      title={atLimit && !soldOut ? `You already have ${inCart} in your cart` : undefined}
      onClick={async () => {
        setBusy(true)
        try {
          await add(product.id, quantity)
          toast(`Added ${quantity} × ${product.name} to your cart`)
        } catch (error) {
          toast(errorMessage(error), 'error')
        } finally {
          setBusy(false)
        }
      }}
    >
      {soldOut ? 'Sold out' : atLimit ? 'Max in cart' : busy ? 'Adding…' : 'Add to cart'}
    </button>
  )
}

export function StockNote({ stock }: { stock: number }) {
  if (stock === 0) return <span className="stock stock-out">Out of stock</span>
  if (stock <= 5) return <span className="stock stock-low">Only {stock} left</span>
  return <span className="stock">In stock</span>
}
