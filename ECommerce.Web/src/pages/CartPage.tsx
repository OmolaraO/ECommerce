import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ordersApi } from '../api'
import { useCart } from '../cart/CartContext'
import { useToast } from '../components/Toast'
import { EmptyState, ErrorBanner, Spinner } from '../components/ui'
import { formatMoney } from '../utils/format'

export function CartPage() {
  const { cart, update, remove, clear, refresh } = useCart()
  const navigate = useNavigate()
  const toast = useToast()
  const [error, setError] = useState<unknown>(null)
  const [busy, setBusy] = useState<string | null>(null)

  async function run(key: string, action: () => Promise<void>) {
    setBusy(key)
    setError(null)
    try {
      await action()
    } catch (e) {
      setError(e)
    } finally {
      setBusy(null)
    }
  }

  if (!cart) return <Spinner />

  if (cart.items.length === 0) {
    return (
      <EmptyState title="Your cart is empty">
        <Link to="/" className="btn btn-primary">
          Browse products
        </Link>
      </EmptyState>
    )
  }

  return (
    <>
      <div className="page-head">
        <h1>Your cart</h1>
        <button className="btn btn-ghost" disabled={busy !== null} onClick={() => run('clear', clear)}>
          Empty cart
        </button>
      </div>

      <ErrorBanner error={error} />

      <div className="card table-wrap">
        <table className="table">
          <thead>
            <tr>
              <th>Product</th>
              <th className="num">Price</th>
              <th>Quantity</th>
              <th className="num">Subtotal</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {cart.items.map((item) => (
              <tr key={item.productId}>
                <td>
                  <Link to={`/products/${item.productId}`}>{item.productName}</Link>
                </td>
                <td className="num">{formatMoney(item.price)}</td>
                <td>
                  <div className="qty">
                    <button
                      className="btn btn-icon"
                      aria-label="Decrease quantity"
                      disabled={busy !== null || item.quantity <= 1}
                      onClick={() => run(item.productId, () => update(item.productId, item.quantity - 1))}
                    >
                      −
                    </button>
                    <span>{item.quantity}</span>
                    <button
                      className="btn btn-icon"
                      aria-label="Increase quantity"
                      disabled={busy !== null}
                      onClick={() => run(item.productId, () => update(item.productId, item.quantity + 1))}
                    >
                      +
                    </button>
                  </div>
                </td>
                <td className="num">{formatMoney(item.lineTotal)}</td>
                <td className="num">
                  <button
                    className="btn btn-link danger"
                    disabled={busy !== null}
                    onClick={() => run(item.productId, () => remove(item.productId))}
                  >
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr>
              <td colSpan={3}>Total</td>
              <td className="num total">{formatMoney(cart.totalAmount)}</td>
              <td />
            </tr>
          </tfoot>
        </table>
      </div>

      <div className="actions-end">
        <Link to="/" className="btn btn-secondary">
          Continue shopping
        </Link>
        <button
          className="btn btn-primary"
          disabled={busy !== null}
          onClick={() =>
            run('checkout', async () => {
              try {
                const order = await ordersApi.checkout()
                toast('Order placed!')
                navigate(`/orders/${order.id}`)
              } finally {
                // Checkout empties the cart on success; on failure prices or stock may have changed
                await refresh()
              }
            })
          }
        >
          {busy === 'checkout' ? 'Placing order…' : `Place order · ${formatMoney(cart.totalAmount)}`}
        </button>
      </div>
    </>
  )
}
