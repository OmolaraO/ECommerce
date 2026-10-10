import type { FieldError } from './types'

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public fieldErrors: FieldError[] = [],
  ) {
    super(message)
  }

  // Validation messages for one field, matched case-insensitively (API sends "Email", forms use "email")
  forField(name: string): string | undefined {
    const matches = this.fieldErrors.filter((e) => e.propertyName.toLowerCase() === name.toLowerCase())
    return matches.length ? matches.map((e) => e.message).join(' ') : undefined
  }
}

let getToken: () => string | null = () => null
let onUnauthorized: () => void = () => {}

export function configureClient(options: { getToken: () => string | null; onUnauthorized: () => void }) {
  getToken = options.getToken
  onUnauthorized = options.onUnauthorized
}

function defaultMessage(status: number): string {
  switch (status) {
    case 400:
      return 'Please check your input.'
    case 401:
      return 'Please sign in to continue.'
    case 403:
      return "You don't have permission to do that."
    case 404:
      return 'Not found.'
    case 502:
    case 503:
    case 504:
      return "Can't reach the API. Make sure it's running on http://localhost:5065."
    default:
      return 'Something went wrong. Please try again.'
  }
}

function parseErrors(data: unknown): FieldError[] {
  if (!data || typeof data !== 'object' || !('errors' in data)) return []
  const errors = (data as { errors: unknown }).errors

  // Our ValidationMappingMiddleware: { errors: [{ propertyName, message }] }
  if (Array.isArray(errors)) return errors as FieldError[]

  // ASP.NET model-binding ProblemDetails: { errors: { Field: ["msg"] } }
  if (errors && typeof errors === 'object') {
    return Object.entries(errors as Record<string, string[]>).flatMap(([propertyName, messages]) =>
      messages.map((message) => ({ propertyName: propertyName.replace(/^\$\./, ''), message })),
    )
  }
  return []
}

export async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`

  let response: Response
  try {
    response = await fetch(path, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError(0, defaultMessage(503))
  }

  const text = await response.text()
  let data: unknown = undefined
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = undefined
    }
  }

  if (!response.ok) {
    // A 401 with a token means it expired or was rejected; drop the session
    if (response.status === 401 && token) onUnauthorized()
    const fieldErrors = parseErrors(data)
    const message = fieldErrors.length
      ? fieldErrors.map((e) => e.message).join(' ')
      : defaultMessage(response.status)
    throw new ApiError(response.status, message, fieldErrors)
  }

  return data as T
}
