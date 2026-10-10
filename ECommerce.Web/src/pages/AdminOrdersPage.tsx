import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { adminApi } from '../api'
import { ORDER_STATUSES, type Order, type OrderStatus } from '../api/types'
import { useToast } from '../components/Toast'
import { ErrorBanner, Spinner, StatusBadge, errorMessage } from '../components/ui'
import { formatDate, formatMoney, shortId } from '../utils/format'

export function AdminOrdersPage() {
  const toast = useToast()
  const [status, setStatus] = useState<OrderStatus | ''>('')
  const [orders, setOrders] = useState<Order[] | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [busy, setBusy] = useState<string | null>(null)

  const load = useCallback(() => {
    setError(null)
    return adminApi
      .orders(status)
      .then((r) => setOrders(r.items))
      .catch(setError)
  }, [status])

  useEffect(() => {
    load()
  }, [load])

  async function setOrderStatus(order: Order, next: OrderStatus) {
    setBusy(order.id)
    try {
      await adminApi.setStatus(order.id, next)
      toast(`Order #${shortId(order.id)} marked ${next.toLowerCase()}`)
      await load()
    } catch (e) {
      toast(errorMessage(e), 'error')
    } finally {
      setBusy(null)
    }
  }

  const pendingTotal = orders?.filter((o) => o.status === 'Pending').length ?? 0

  return (
    <>
      <div className="page-head">
        <h1>All orders</h1>
        {orders && <span className="muted">{pendingTotal} pending</span>}
      </div>

      <div className="filters">
        <div className="segmented" role="group" aria-label="Filter by status">
          {(['', ...ORDER_STATUSES] as const).map((s) => (
            <button key={s || 'all'} className={status === s ? 'active' : ''} onClick={() => setStatus(s)}>
              {s || 'All'}
            </button>
          ))}
        </div>
      </div>

      <ErrorBanner error={error} />

      {!orders ? (
        !error && <Spinner />
      ) : orders.length === 0 ? (
        <p className="muted">No {status ? status.toLowerCase() : ''} orders.</p>
      ) : (
        <div className="card table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Order</th>
                <th>Placed</th>
                <th>Customer</th>
                <th className="num">Items</th>
                <th className="num">Total</th>
                <th>Status</th>
                <th />
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
                  <td className="mono muted" title={o.userId}>
                    {shortId(o.userId)}
                  </td>
                  <td className="num">{o.items.reduce((n, i) => n + i.quantity, 0)}</td>
                  <td className="num">{formatMoney(o.totalAmount)}</td>
                  <td>
                    <StatusBadge status={o.status} />
                  </td>
                  <td className="num row-actions">
                    {o.status === 'Pending' && (
                      <>
                        <button
                          className="btn btn-link"
                          disabled={busy !== null}
                          onClick={() => setOrderStatus(o, 'Completed')}
                        >
                          Complete
                        </button>
                        <button
                          className="btn btn-link danger"
                          disabled={busy !== null}
                          onClick={() => setOrderStatus(o, 'Cancelled')}
                        >
                          Cancel
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  )
}
