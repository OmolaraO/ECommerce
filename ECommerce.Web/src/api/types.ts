// Mirrors ECommerce.Contracts; the API serializes as camelCase

export interface AuthResponse {
  userId: string
  userName: string
  email: string
  role: string
  token: string
  expiresAt: string
}

export interface Product {
  id: string
  name: string
  description: string
  price: number
  stockQuantity: number
  category: string
  createdAt: string
}

export interface ProductsResponse {
  items: Product[]
  page: number
  pageSize: number
  total: number
  hasNextPage: boolean
}

export interface ProductInput {
  name: string
  description: string
  price: number
  stockQuantity: number
  category: string
}

export interface ProductQuery {
  name?: string
  category?: string
  minPrice?: string
  maxPrice?: string
  page?: number
  pageSize?: number
}

export interface CartItem {
  productId: string
  productName: string
  price: number
  quantity: number
  lineTotal: number
}

export interface Cart {
  id: string
  items: CartItem[]
  totalAmount: number
}

export interface OrderItem {
  productId: string
  productName: string
  quantity: number
  priceAtPurchase: number
}

export interface Order {
  id: string
  userId: string
  status: OrderStatus
  totalAmount: number
  createdAt: string
  items: OrderItem[]
}

export interface OrdersResponse {
  items: Order[]
}

export interface FieldError {
  propertyName: string
  message: string
}

// Must match the ProductCategory and OrderStatus enums in ECommerce.Application
export const CATEGORIES = [
  'Electronics',
  'Clothing',
  'Food',
  'Books',
  'HomeAndGarden',
  'Sports',
  'Toys',
  'Others',
] as const

export const ORDER_STATUSES = ['Pending', 'Completed', 'Cancelled'] as const
export type OrderStatus = (typeof ORDER_STATUSES)[number]
