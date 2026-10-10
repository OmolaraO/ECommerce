import { Link, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { RequireAuth } from './components/RequireAuth'
import { EmptyState } from './components/ui'
import { AdminOrdersPage } from './pages/AdminOrdersPage'
import { AdminProductsPage } from './pages/AdminProductsPage'
import { LoginPage, RegisterPage } from './pages/AuthPages'
import { CartPage } from './pages/CartPage'
import { OrderPage, OrdersPage } from './pages/OrdersPages'
import { ProductPage } from './pages/ProductPage'
import { ShopPage } from './pages/ShopPage'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<ShopPage />} />
        <Route path="products/:id" element={<ProductPage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="cart" element={<RequireAuth><CartPage /></RequireAuth>} />
        <Route path="orders" element={<RequireAuth><OrdersPage /></RequireAuth>} />
        <Route path="orders/:id" element={<RequireAuth><OrderPage /></RequireAuth>} />
        <Route path="admin/products" element={<RequireAuth admin><AdminProductsPage /></RequireAuth>} />
        <Route path="admin/orders" element={<RequireAuth admin><AdminOrdersPage /></RequireAuth>} />
        <Route
          path="*"
          element={
            <EmptyState title="Page not found">
              <Link to="/">Back to the shop</Link>
            </EmptyState>
          }
        />
      </Route>
    </Routes>
  )
}
