import { request } from './client'
import type {
  AuthResponse,
  Cart,
  Order,
  OrdersResponse,
  OrderStatus,
  Product,
  ProductInput,
  ProductQuery,
  ProductsResponse,
} from './types'

function queryString(params: Record<string, string | number | undefined>): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') search.set(key, String(value))
  }
  const s = search.toString()
  return s ? `?${s}` : ''
}

export const authApi = {
  login: (email: string, password: string) =>
    request<AuthResponse>('POST', '/api/auth/login', { email, password }),
  register: (userName: string, email: string, password: string) =>
    request<AuthResponse>('POST', '/api/auth/register', { userName, email, password }),
}

export const productsApi = {
  list: (query: ProductQuery) => request<ProductsResponse>('GET', `/api/products${queryString({ ...query })}`),
  get: (id: string) => request<Product>('GET', `/api/products/${id}`),
  create: (input: ProductInput) => request<Product>('POST', '/api/products', input),
  update: (id: string, input: ProductInput) => request<Product>('PUT', `/api/products/${id}`, input),
  remove: (id: string) => request<void>('DELETE', `/api/products/${id}`),
}

export const cartApi = {
  get: () => request<Cart>('GET', '/api/cart'),
  add: (productId: string, quantity: number) => request<Cart>('POST', '/api/cart/items', { productId, quantity }),
  update: (productId: string, quantity: number) =>
    request<Cart>('PUT', `/api/cart/items/${productId}`, { quantity }),
  remove: (productId: string) => request<void>('DELETE', `/api/cart/items/${productId}`),
  clear: () => request<void>('DELETE', '/api/cart'),
}

export const ordersApi = {
  checkout: () => request<Order>('POST', '/api/orders'),
  mine: () => request<OrdersResponse>('GET', '/api/orders'),
  get: (id: string) => request<Order>('GET', `/api/orders/${id}`),
  cancel: (id: string) => request<Order>('POST', `/api/orders/${id}/cancel`),
}

export const adminApi = {
  orders: (status?: OrderStatus | '') => request<OrdersResponse>('GET', `/api/admin/orders${queryString({ status })}`),
  setStatus: (id: string, status: OrderStatus) =>
    request<Order>('PUT', `/api/admin/orders/${id}/status`, { status }),
}
