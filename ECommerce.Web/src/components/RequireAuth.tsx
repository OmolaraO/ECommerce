import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { EmptyState } from './ui'

export function RequireAuth({ children, admin = false }: { children: ReactNode; admin?: boolean }) {
  const { user, isAdmin } = useAuth()
  const location = useLocation()

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
  }
  if (admin && !isAdmin) {
    return (
      <EmptyState title="Admins only">
        <p>You need an admin account to see this page.</p>
      </EmptyState>
    )
  }
  return <>{children}</>
}
