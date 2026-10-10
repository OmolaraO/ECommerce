import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { authApi } from '../api'
import { configureClient } from '../api/client'
import type { AuthResponse } from '../api/types'

const STORAGE_KEY = 'ecommerce.auth'

interface AuthContextValue {
  user: AuthResponse | null
  isAdmin: boolean
  login: (email: string, password: string) => Promise<void>
  register: (userName: string, email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function loadStoredUser(): AuthResponse | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    const user = JSON.parse(raw) as AuthResponse
    return new Date(user.expiresAt).getTime() > Date.now() ? user : null
  } catch {
    return null
  }
}

function storeUser(user: AuthResponse | null) {
  try {
    if (user) localStorage.setItem(STORAGE_KEY, JSON.stringify(user))
    else localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Storage can be unavailable (private mode); the session just won't survive a reload
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthResponse | null>(loadStoredUser)
  const userRef = useRef(user)
  userRef.current = user

  const setSession = useCallback((next: AuthResponse | null) => {
    storeUser(next)
    setUser(next)
  }, [])

  const logout = useCallback(() => setSession(null), [setSession])

  // Configure synchronously so the very first requests already carry the token
  configureClient({ getToken: () => userRef.current?.token ?? null, onUnauthorized: logout })

  // Sign out when the token expires instead of waiting for a 401
  useEffect(() => {
    if (!user) return
    const ms = new Date(user.expiresAt).getTime() - Date.now()
    const timer = window.setTimeout(logout, Math.max(ms, 0))
    return () => window.clearTimeout(timer)
  }, [user, logout])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAdmin: user?.role === 'Admin',
      login: async (email, password) => setSession(await authApi.login(email, password)),
      register: async (userName, email, password) => setSession(await authApi.register(userName, email, password)),
      logout,
    }),
    [user, setSession, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
