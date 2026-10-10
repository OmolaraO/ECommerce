import type { ReactNode } from 'react'
import { ApiError } from '../api/client'
import { humanize } from '../utils/format'

export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) return error.message
  if (error instanceof Error) return error.message
  return 'Something went wrong.'
}

export function ErrorBanner({ error }: { error: unknown }) {
  if (!error) return null
  return <div className="alert alert-error">{errorMessage(error)}</div>
}

export function Spinner({ label = 'Loading…' }: { label?: string }) {
  return (
    <div className="loading">
      <span className="spinner" aria-hidden="true" />
      {label}
    </div>
  )
}

export function StatusBadge({ status }: { status: string }) {
  return <span className={`badge badge-${status.toLowerCase()}`}>{status}</span>
}

export function CategoryTag({ category }: { category: string }) {
  return <span className="tag">{humanize(category)}</span>
}

export function EmptyState({ title, children }: { title: string; children?: ReactNode }) {
  return (
    <div className="empty">
      <h2>{title}</h2>
      {children}
    </div>
  )
}

export function Field({
  label,
  error,
  children,
}: {
  label: string
  error?: string
  children: ReactNode
}) {
  return (
    <label className={`field${error ? ' field-invalid' : ''}`}>
      <span className="field-label">{label}</span>
      {children}
      {error && <span className="field-error">{error}</span>}
    </label>
  )
}
