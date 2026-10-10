import { useEffect, useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { productsApi } from '../api'
import { CATEGORIES, type ProductsResponse } from '../api/types'
import { AddToCart, StockNote } from '../components/AddToCart'
import { CategoryTag, EmptyState, ErrorBanner, Spinner } from '../components/ui'
import { formatMoney, humanize } from '../utils/format'

const PAGE_SIZE = 12

export function ShopPage() {
  const [params, setParams] = useSearchParams()
  const [data, setData] = useState<ProductsResponse | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [loading, setLoading] = useState(true)

  const name = params.get('name') ?? ''
  const category = params.get('category') ?? ''
  const minPrice = params.get('minPrice') ?? ''
  const maxPrice = params.get('maxPrice') ?? ''
  const page = Math.max(1, Number(params.get('page')) || 1)

  // Search box is local state so typing doesn't refetch on every keystroke
  const [search, setSearch] = useState(name)
  const [min, setMin] = useState(minPrice)
  const [max, setMax] = useState(maxPrice)
  useEffect(() => {
    setSearch(name)
    setMin(minPrice)
    setMax(maxPrice)
  }, [name, minPrice, maxPrice])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)
    productsApi
      .list({ name, category, minPrice, maxPrice, page, pageSize: PAGE_SIZE })
      .then((result) => !cancelled && setData(result))
      .catch((e) => !cancelled && setError(e))
      .finally(() => !cancelled && setLoading(false))
    return () => {
      cancelled = true
    }
  }, [name, category, minPrice, maxPrice, page])

  function update(changes: Record<string, string>) {
    const next = new URLSearchParams(params)
    for (const [key, value] of Object.entries(changes)) {
      if (value) next.set(key, value)
      else next.delete(key)
    }
    if (!('page' in changes)) next.delete('page')
    setParams(next)
  }

  function onSubmit(e: FormEvent) {
    e.preventDefault()
    update({ name: search.trim(), minPrice: min, maxPrice: max })
  }

  const hasFilters = Boolean(name || category || minPrice || maxPrice)

  return (
    <>
      <div className="page-head">
        <h1>Shop</h1>
        {data && <span className="muted">{data.total} products</span>}
      </div>

      <form className="filters" onSubmit={onSubmit}>
        <input
          type="search"
          placeholder="Search products…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          aria-label="Search products"
        />
        <select value={category} onChange={(e) => update({ category: e.target.value })} aria-label="Category">
          <option value="">All categories</option>
          {CATEGORIES.map((c) => (
            <option key={c} value={c}>
              {humanize(c)}
            </option>
          ))}
        </select>
        <input
          type="number"
          min="0"
          step="0.01"
          placeholder="Min price"
          value={min}
          onChange={(e) => setMin(e.target.value)}
          aria-label="Minimum price"
        />
        <input
          type="number"
          min="0"
          step="0.01"
          placeholder="Max price"
          value={max}
          onChange={(e) => setMax(e.target.value)}
          aria-label="Maximum price"
        />
        <button className="btn btn-primary" type="submit">
          Search
        </button>
        {hasFilters && (
          <button className="btn btn-ghost" type="button" onClick={() => setParams(new URLSearchParams())}>
            Clear
          </button>
        )}
      </form>

      <ErrorBanner error={error} />

      {loading && !data ? (
        <Spinner />
      ) : data && data.items.length === 0 ? (
        <EmptyState title="No products found">
          <p>{hasFilters ? 'Try a different search or clear the filters.' : 'An admin needs to add some products first.'}</p>
        </EmptyState>
      ) : (
        data && (
          <>
            <div className={`grid${loading ? ' is-loading' : ''}`}>
              {data.items.map((p) => (
                <article key={p.id} className="card product-card">
                  <div className="product-card-top">
                    <CategoryTag category={p.category} />
                    <StockNote stock={p.stockQuantity} />
                  </div>
                  <h3>
                    <Link to={`/products/${p.id}`}>{p.name}</Link>
                  </h3>
                  <p className="muted clamp">{p.description}</p>
                  <div className="product-card-bottom">
                    <span className="price">{formatMoney(p.price)}</span>
                    <AddToCart product={p} />
                  </div>
                </article>
              ))}
            </div>

            {data.total > data.pageSize && (
              <div className="pager">
                <button
                  className="btn btn-secondary"
                  disabled={page <= 1}
                  onClick={() => update({ page: String(page - 1) })}
                >
                  ← Previous
                </button>
                <span className="muted">
                  Page {page} of {Math.ceil(data.total / data.pageSize)}
                </span>
                <button
                  className="btn btn-secondary"
                  disabled={!data.hasNextPage}
                  onClick={() => update({ page: String(page + 1) })}
                >
                  Next →
                </button>
              </div>
            )}
          </>
        )
      )}
    </>
  )
}
