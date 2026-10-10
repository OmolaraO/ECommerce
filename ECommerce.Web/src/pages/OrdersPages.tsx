import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ordersApi } from '../api'
import { ApiError } from '../api/client'
import type { Order } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { useToast } from '../components/Toast'
import { EmptyState, ErrorBanner, Spinner, StatusBadge } from '../components/ui'
import { formatDate, formatMoney, shortId } from '../utils/format'

export function OrdersPage() {
  const [orders, setOrders] = useState<Order[] | null>(null)
  const [error, setError] = useState<unknown>(null)

  useEffect(() => {
    ordersApi
      .mine()
      .then((r) => setOrders(r.items))
      .catch(setError)
  }, [])

  if (error) return <ErrorBanner error={error} />
  if (!orders) return <Spinner />

  if (orders.length === 0) {
    return (
      <EmptyState title="No orders yet">
        <Link to="/" className="btn btn-primary">
          Start shopping
        </Link>
      </EmptyState>
    )
  }

  return (
    <>
      <div className="page-head">
        <h1>My orders</h1>
      </div>
      <OrdersTable orders={orders} />
    </>
  )
}

export function OrdersTable({ orders, showCustomer = false }: { orders: Order[]; showCustomer?: boolean }) {
  return (
    <div className="card table-wrap">
      <table className="table">
        <thead>
          <tr>
            <th>Order</th>
            <th>Placed</th>
            {showCustomer && <th>Customer</th>}
            <th className="num">Items</th>
            <th className="num">Total</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((o) => (
            <tr key={o.id}>
              <td>
                <Link to={`/orders/${o.id}`} className="mono">
                  #{shortId(o.id)}
                </Link>
              </td>
              <td>{formatDate(o.createdAt)}</td>
              {showCustomer && <td className="mono muted">{shortId(o.userId)}</td>}
              <td className="num">{o.items.reduce((n, i) => n + i.quantity, 0)}</td>
              <td className="num">{formatMoney(o.totalAmount)}</td>
              <td>
                <StatusBadge status={o.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export function OrderPage() {
  const { id = '' } = useParams()
  const { isAdmin } = useAuth()
  const toast = useToast()
  const [order, setOrder] = useState<Order | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [confirming, setConfirming] = useState(false)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    setOrder(null)
    setError(null)
    ordersApi.get(id).then(setOrder).catch(setError)
  }, [id])

  if (error instanceof ApiError && error.status === 404) {
    return (
      <EmptyState title="Order not found">
        <Link to="/orders">Back to my orders</Link>
      </EmptyState>
    )
  }
  if (!order) return error ? <ErrorBanner error={error} /> : <Spinner />

  async function cancel() {
    setBusy(true)
    setError(null)
    try {
      setOrder(await ordersApi.cancel(id))
      toast('Order cancelled')
    } catch (e) {
      setError(e)
    } finally {
      setBusy(false)
      setConfirming(false)
    }
  }

  return (
    <>
      <Link to={isAdmin ? '/admin/orders' : '/orders'} className="back">
        ← Back to orders
      </Link>
      <div className="page-head">
        <h1>
          Order <span className="mono">#{shortId(order.id)}</span>
        </h1>
        <StatusBadge status={order.status} />
      </div>
      <p className="muted">Placed {formatDate(order.createdAt)}</p>

      <ErrorBanner error={error} />

      <div className="card table-wrap">
        <table className="table">
          <thead>
            <tr>
              <th>Product</th>
              <th className="num">Price</th>
              <th className="num">Qty</th>
              <th className="num">Subtotal</th>
            </tr>
          </thead>
          <tbody>
            {order.items.map((item) => (
              <tr key={item.productId}>
                <td>{item.productName}</td>
                <td className="num">{formatMoney(item.priceAtPurchase)}</td>
                <td className="num">{item.quantity}</td>
                <td className="num">{formatMoney(item.priceAtPurchase * item.quantity)}</td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr>
              <td colSpan={3}>Total</td>
              <td className="num total">{formatMoney(order.totalAmount)}</td>
            </tr>
          </tfoot>
        </table>
      </div>

      {order.status === 'Pending' && (
        <div className="actions-end">
          {confirming ? (
            <>
              <span className="muted">Cancel this order? Items go back into stock.</span>
              <button className="btn btn-secondary" disabled={busy} onClick={() => setConfirming(false)}>
                Keep order
              </button>
              <button className="btn btn-danger" disabled={busy} onClick={cancel}>
                {busy ? 'Cancelling…' : 'Yes, cancel'}
              </button>
            </>
          ) : (
            <button className="btn btn-secondary" onClick={() => setConfirming(true)}>
              Cancel order
            </button>
          )}
        </div>
      )}
    </>
  )
}
