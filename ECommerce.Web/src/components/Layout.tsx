import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { useCart } from '../cart/CartContext'

export function Layout() {
  const { user, isAdmin, logout } = useAuth()
  const { itemCount } = useCart()
  const navigate = useNavigate()

  return (
    <>
      <header className="header">
        <div className="container header-inner">
          <Link to="/" className="brand">
            E‑Commerce
          </Link>
          <nav className="nav">
            <NavLink to="/" end>
              Shop
            </NavLink>
            {user && (
              <>
                <NavLink to="/cart">
                  Cart{itemCount > 0 && <span className="count">{itemCount}</span>}
                </NavLink>
                <NavLink to="/orders">My orders</NavLink>
              </>
            )}
            {isAdmin && (
              <>
                <NavLink to="/admin/products">Manage products</NavLink>
                <NavLink to="/admin/orders">All orders</NavLink>
              </>
            )}
          </nav>
          <div className="account">
            {user ? (
              <>
                <span className="muted">
                  {user.userName}
                  {isAdmin && <span className="tag tag-admin">Admin</span>}
                </span>
                <button
                  className="btn btn-ghost"
                  onClick={() => {
                    logout()
                    navigate('/')
                  }}
                >
                  Sign out
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="btn btn-ghost">
                  Sign in
                </Link>
                <Link to="/register" className="btn btn-primary">
                  Create account
                </Link>
              </>
            )}
          </div>
        </div>
      </header>
      <main className="container main">
        <Outlet />
      </main>
    </>
  )
}
