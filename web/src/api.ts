// Mirrors the backend DTOs in backend/src/EatUp.Api/Endpoints.cs.
// MVP: hand-written; replace with types generated from OpenAPI when the API grows.

import { strings } from './strings'

export type StorageLocation = 'Fridge' | 'Freezer' | 'Cupboard' | 'Counter'
export type Urgency = 'Expired' | 'Today' | 'Soon' | 'Later'
export type ItemStatus = 'Active' | 'Used' | 'Discarded'
export type ConsumeOutcome = 'Used' | 'Discarded'
export type Language = 'English' | 'Norwegian' | 'Finnish'
export type DietType = 'Omnivore' | 'Flexitarian' | 'Pescatarian' | 'Vegetarian' | 'Vegan'
export type Allergen =
  | 'Gluten' | 'Crustaceans' | 'Eggs' | 'Fish' | 'Peanuts' | 'Soy' | 'Milk'
  | 'TreeNuts' | 'Celery' | 'Mustard' | 'Sesame' | 'Sulphites' | 'Lupin' | 'Molluscs'

/** ISO date, `YYYY-MM-DD`. */
export type IsoDate = string

export interface Item {
  id: number
  name: string
  ingredientKey: string | null
  quantity: number
  unit: string
  location: StorageLocation
  expiryDate: IsoDate
  openedDate: IsoDate | null
  effectiveExpiry: IsoDate
  daysLeft: number
  urgency: Urgency
  status: ItemStatus
}

export interface ItemInput {
  name: string
  ingredientKey: string | null
  quantity: number
  unit: string
  location: StorageLocation
  expiryDate: IsoDate | null
  openedDate: IsoDate | null
}

export interface Profile {
  diet: DietType
  allergens: Allergen[]
  /** `HH:mm:ss` */
  notificationTime: string
  timeZone: string
  notificationsEnabled: boolean
  /** Null until the app first reports the browser language. */
  language: Language | null
}

export interface Ingredient {
  key: string
  name: string
  /** Lower-case names and aliases in every language, words separated by single spaces. */
  aliases: string[]
  category: string
  allergens: Allergen[]
  shelfLifeDays: Partial<Record<StorageLocation, number>>
  openedLocation: StorageLocation | null
}

/** A scanned product. Null fields weren't found and are left for the user. */
export interface Product {
  name: string | null
  productName: string | null
  quantity: number | null
  unit: string | null
}

export interface PantryItem {
  id: number
  name: string
  ingredientKey: string | null
  daysLeft: number
}

export interface Suggestion {
  key: string
  title: string
  description: string
  minutes: number
  steps: string[]
  usesItems: PantryItem[]
  missingIngredients: string[]
  isPlantBased: boolean
}

export interface DailyCheckResult {
  sent: number
  title: string | null
  body: string | null
}

export class ApiError extends Error {
  constructor(message: string, readonly status: number) {
    super(message)
  }
}

type Problem = { title?: string; errors?: Record<string, string[]> } | null

// The backend writes English; the message is chosen here so it is always in the user's language.
function problemMessage(problem: Problem, status: number): string {
  const field = Object.keys(problem?.errors ?? {})[0]
  return (field && strings.apiErrors[field]) || strings.errors.byStatus[status] || strings.errors.requestFailed(status)
}

async function request<T>(path: string, init?: RequestInit & { json?: unknown }): Promise<T> {
  const { json, ...rest } = init ?? {}
  let response: Response
  try {
    response = await fetch(`/api${path}`, {
      ...rest,
      // X-EatUp: the backend rejects writes without it, so other websites can't send them (see Endpoints.cs).
      headers: {
        'X-EatUp': '1',
        ...(json === undefined ? {} : { 'Content-Type': 'application/json' }),
        ...rest.headers,
      },
      body: json === undefined ? rest.body : JSON.stringify(json),
    })
  } catch {
    throw new Error(strings.errors.offline)
  }
  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new ApiError(problemMessage(problem, response.status), response.status)
  }
  return response.status === 204 ? (undefined as T) : response.json()
}

export const api = {
  items: (status: ItemStatus = 'Active') => request<Item[]>(`/items?status=${status}`),
  item: (id: number) => request<Item>(`/items/${id}`),
  createItem: (input: ItemInput) => request<Item>('/items', { method: 'POST', json: input }),
  updateItem: (id: number, input: ItemInput) => request<Item>(`/items/${id}`, { method: 'PUT', json: input }),
  deleteItem: (id: number) => request<void>(`/items/${id}`, { method: 'DELETE' }),
  consume: (id: number, outcome: ConsumeOutcome, amount?: number) =>
    request<Item>(`/items/${id}/consume`, { method: 'POST', json: { outcome, amount: amount ?? null } }),

  profile: () => request<Profile>('/profile'),
  /** Only the given fields are changed. */
  updateProfile: (changes: Partial<Profile>) => request<Profile>('/profile', { method: 'PATCH', json: changes }),

  ingredients: (search: string) => request<Ingredient[]>(`/ingredients?search=${encodeURIComponent(search)}`),
  product: (barcode: string) => request<Product>(`/products/${encodeURIComponent(barcode)}`),
  suggestions: () => request<Suggestion[]>('/suggestions'),

  vapidPublicKey: () => request<{ publicKey: string }>('/push/vapid-public-key').then((r) => r.publicKey),
  subscribe: (subscription: PushSubscriptionJSON) => request<void>('/push/subscriptions', { method: 'POST', json: subscription }),
  unsubscribe: (endpoint: string) => request<void>('/push/subscriptions', { method: 'DELETE', json: { endpoint } }),

  dev: {
    testPush: () => request<{ sent: number }>('/dev/push/test', { method: 'POST' }),
    runDailyCheck: () => request<DailyCheckResult>('/dev/run-daily-check', { method: 'POST' }),
    resetDemoData: () => request<void>('/dev/reset-demo-data', { method: 'POST' }),
  },
}
