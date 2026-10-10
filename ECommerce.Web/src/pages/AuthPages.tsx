import { useState, type FormEvent } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { useToast } from '../components/Toast'
import { Field, errorMessage } from '../components/ui'

function useRedirectTarget() {
  const location = useLocation()
  return (location.state as { from?: string } | null)?.from ?? '/'
}

export function LoginPage() {
  const { user, login } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const target = useRedirectTarget()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  if (user) return <Navigate to={target} replace />

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await login(email, password)
      toast('Welcome back!')
      navigate(target, { replace: true })
    } catch (err) {
      setError(err instanceof ApiError && err.status === 401 ? 'Incorrect email or password.' : errorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-card card">
      <h1>Sign in</h1>
      {error && <div className="alert alert-error">{error}</div>}
      <form onSubmit={onSubmit} className="stack">
        <Field label="Email">
          <input type="email" autoComplete="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </Field>
        <Field label="Password">
          <input
            type="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </Field>
        <button className="btn btn-primary btn-block" disabled={busy}>
          {busy ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
      <p className="muted center">
        New here?{' '}
        <Link to="/register" state={{ from: target }}>
          Create an account
        </Link>
      </p>
    </div>
  )
}

export function RegisterPage() {
  const { user, register } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const target = useRedirectTarget()
  const [form, setForm] = useState({ userName: '', email: '', password: '' })
  const [error, setError] = useState<ApiError | Error | null>(null)
  const [busy, setBusy] = useState(false)

  if (user) return <Navigate to={target} replace />

  const fieldError = (name: string) => (error instanceof ApiError ? error.forField(name) : undefined)
  const set = (key: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((f) => ({ ...f, [key]: e.target.value }))

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await register(form.userName, form.email, form.password)
      toast('Account created — you are signed in.')
      navigate(target, { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err : new Error(errorMessage(err)))
    } finally {
      setBusy(false)
    }
  }

  const generalError = error && !(error instanceof ApiError && error.fieldErrors.length) ? error.message : null

  return (
    <div className="auth-card card">
      <h1>Create account</h1>
      {generalError && <div className="alert alert-error">{generalError}</div>}
      <form onSubmit={onSubmit} className="stack">
        <Field label="Username" error={fieldError('userName')}>
          <input autoComplete="username" required minLength={3} maxLength={100} value={form.userName} onChange={set('userName')} />
        </Field>
        <Field label="Email" error={fieldError('email')}>
          <input type="email" autoComplete="email" required value={form.email} onChange={set('email')} />
        </Field>
        <Field label="Password" error={fieldError('password')}>
          <input
            type="password"
            autoComplete="new-password"
            required
            minLength={8}
            value={form.password}
            onChange={set('password')}
          />
          <span className="hint">At least 8 characters.</span>
        </Field>
        <button className="btn btn-primary btn-block" disabled={busy}>
          {busy ? 'Creating account…' : 'Create account'}
        </button>
      </form>
      <p className="muted center">
        Already have an account?{' '}
        <Link to="/login" state={{ from: target }}>
          Sign in
        </Link>
      </p>
    </div>
  )
}
