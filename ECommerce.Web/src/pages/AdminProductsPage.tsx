import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { productsApi } from '../api'
import { ApiError } from '../api/client'
import { CATEGORIES, type Product, type ProductInput, type ProductsResponse } from '../api/types'
import { useToast } from '../components/Toast'
import { ErrorBanner, Field, Spinner, errorMessage } from '../components/ui'
import { formatMoney, humanize } from '../utils/format'

const PAGE_SIZE = 50

type Editing = { mode: 'create' } | { mode: 'edit'; product: Product } | null

export function AdminProductsPage() {
  const toast = useToast()
  const [data, setData] = useState<ProductsResponse | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [query, setQuery] = useState('')
  const [editing, setEditing] = useState<Editing>(null)
  const [confirmDelete, setConfirmDelete] = useState<string | null>(null)

  const load = useCallback(() => {
    setError(null)
    return productsApi
      .list({ name: query, page, pageSize: PAGE_SIZE })
      .then(setData)
      .catch(setError)
  }, [query, page])

  useEffect(() => {
    load()
  }, [load])

  async function onDelete(product: Product) {
    try {
      await productsApi.remove(product.id)
      toast(`Deleted ${product.name}`)
      setConfirmDelete(null)
      await load()
    } catch (e) {
      toast(errorMessage(e), 'error')
      setConfirmDelete(null)
    }
  }

  return (
    <>
      <div className="page-head">
        <h1>Manage products</h1>
        <button className="btn btn-primary" onClick={() => setEditing({ mode: 'create' })}>
          + New product
        </button>
      </div>

      {editing && (
        <ProductForm
          key={editing.mode === 'edit' ? editing.product.id : 'new'}
          product={editing.mode === 'edit' ? editing.product : undefined}
          onCancel={() => setEditing(null)}
          onSaved={async (saved) => {
            toast(editing.mode === 'edit' ? `Saved ${saved.name}` : `Created ${saved.name}`)
            setEditing(null)
            await load()
          }}
        />
      )}

      <form
        className="filters"
        onSubmit={(e) => {
          e.preventDefault()
          setPage(1)
          setQuery(search.trim())
        }}
      >
        <input type="search" placeholder="Search by name…" value={search} onChange={(e) => setSearch(e.target.value)} />
        <button className="btn btn-secondary">Search</button>
      </form>

      <ErrorBanner error={error} />

      {!data ? (
        !error && <Spinner />
      ) : data.items.length === 0 ? (
        <p className="muted">{query ? 'No products match that search.' : 'No products yet. Create the first one.'}</p>
      ) : (
        <>
          <div className="card table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Category</th>
                  <th className="num">Price</th>
                  <th className="num">Stock</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {data.items.map((p) => (
                  <tr key={p.id}>
                    <td>
                      <Link to={`/products/${p.id}`}>{p.name}</Link>
                    </td>
                    <td>{humanize(p.category)}</td>
                    <td className="num">{formatMoney(p.price)}</td>
                    <td className={`num${p.stockQuantity === 0 ? ' danger' : ''}`}>{p.stockQuantity}</td>
                    <td className="num row-actions">
                      {confirmDelete === p.id ? (
                        <>
                          <span className="muted">Delete?</span>
                          <button className="btn btn-link" onClick={() => setConfirmDelete(null)}>
                            No
                          </button>
                          <button className="btn btn-link danger" onClick={() => onDelete(p)}>
                            Yes, delete
                          </button>
                        </>
                      ) : (
                        <>
                          <button className="btn btn-link" onClick={() => setEditing({ mode: 'edit', product: p })}>
                            Edit
                          </button>
                          <button className="btn btn-link danger" onClick={() => setConfirmDelete(p.id)}>
                            Delete
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {data.total > data.pageSize && (
            <div className="pager">
              <button className="btn btn-secondary" disabled={page <= 1} onClick={() => setPage(page - 1)}>
                ← Previous
              </button>
              <span className="muted">
                Page {page} of {Math.ceil(data.total / data.pageSize)}
              </span>
              <button className="btn btn-secondary" disabled={!data.hasNextPage} onClick={() => setPage(page + 1)}>
                Next →
              </button>
            </div>
          )}
        </>
      )}
    </>
  )
}

function ProductForm({
  product,
  onCancel,
  onSaved,
}: {
  product?: Product
  onCancel: () => void
  onSaved: (product: Product) => void | Promise<void>
}) {
  const [form, setForm] = useState({
    name: product?.name ?? '',
    description: product?.description ?? '',
    price: product ? String(product.price) : '',
    stockQuantity: product ? String(product.stockQuantity) : '0',
    category: product?.category ?? CATEGORIES[0],
  })
  const [error, setError] = useState<unknown>(null)
  const [busy, setBusy] = useState(false)

  const fieldError = (name: string) => (error instanceof ApiError ? error.forField(name) : undefined)
  const set =
    (key: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) =>
      setForm((f) => ({ ...f, [key]: e.target.value }))

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    const input: ProductInput = {
      name: form.name.trim(),
      description: form.description.trim(),
      price: Number(form.price),
      stockQuantity: Number(form.stockQuantity),
      category: form.category,
    }
    try {
      const saved = product ? await productsApi.update(product.id, input) : await productsApi.create(input)
      await onSaved(saved)
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  const showGeneral = Boolean(error) && !(error instanceof ApiError && error.fieldErrors.length)

  return (
    <form className="card panel stack" onSubmit={onSubmit}>
      <h2>{product ? `Edit ${product.name}` : 'New product'}</h2>
      {showGeneral && <ErrorBanner error={error} />}
      <div className="form-grid">
        <Field label="Name" error={fieldError('name')}>
          <input required maxLength={255} value={form.name} onChange={set('name')} />
        </Field>
        <Field label="Category" error={fieldError('category')}>
          <select value={form.category} onChange={set('category')}>
            {CATEGORIES.map((c) => (
              <option key={c} value={c}>
                {humanize(c)}
              </option>
            ))}
          </select>
        </Field>
        <Field label="Price" error={fieldError('price')}>
          <input type="number" required min="0.01" step="0.01" value={form.price} onChange={set('price')} />
        </Field>
        <Field label="Stock quantity" error={fieldError('stockQuantity')}>
          <input type="number" required min="0" step="1" value={form.stockQuantity} onChange={set('stockQuantity')} />
        </Field>
      </div>
      <Field label="Description" error={fieldError('description')}>
        <textarea rows={3} maxLength={1000} value={form.description} onChange={set('description')} />
      </Field>
      <div className="actions-end">
        <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={busy}>
          Cancel
        </button>
        <button className="btn btn-primary" disabled={busy}>
          {busy ? 'Saving…' : product ? 'Save changes' : 'Create product'}
        </button>
      </div>
    </form>
  )
}
