import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { productsApi } from '../api'
import { ApiError } from '../api/client'
import type { Product } from '../api/types'
import { AddToCart, StockNote } from '../components/AddToCart'
import { CategoryTag, EmptyState, ErrorBanner, Spinner } from '../components/ui'
import { formatMoney } from '../utils/format'

export function ProductPage() {
  const { id = '' } = useParams()
  const [product, setProduct] = useState<Product | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [quantity, setQuantity] = useState(1)

  useEffect(() => {
    setProduct(null)
    setError(null)
    productsApi.get(id).then(setProduct).catch(setError)
  }, [id])

  if (error instanceof ApiError && error.status === 404) {
    return (
      <EmptyState title="Product not found">
        <Link to="/">Back to the shop</Link>
      </EmptyState>
    )
  }
  if (error) return <ErrorBanner error={error} />
  if (!product) return <Spinner />

  const maxQty = Math.min(product.stockQuantity, 100)

  return (
    <>
      <Link to="/" className="back">
        ← Back to shop
      </Link>
      <article className="card product-detail">
        <div className="product-card-top">
          <CategoryTag category={product.category} />
          <StockNote stock={product.stockQuantity} />
        </div>
        <h1>{product.name}</h1>
        <p className="price price-lg">{formatMoney(product.price)}</p>
        <p className="description">{product.description || <span className="muted">No description.</span>}</p>
        {maxQty > 0 && (
          <div className="buy-row">
            <label className="inline-field">
              Quantity
              <select value={quantity} onChange={(e) => setQuantity(Number(e.target.value))}>
                {Array.from({ length: maxQty }, (_, i) => i + 1).map((n) => (
                  <option key={n} value={n}>
                    {n}
                  </option>
                ))}
              </select>
            </label>
            <AddToCart product={product} quantity={quantity} />
          </div>
        )}
      </article>
    </>
  )
}
