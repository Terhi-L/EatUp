import createClient from 'openapi-fetch'
import type { components, paths } from './api-schema'
import { strings } from './strings'

// Generated from the backend's OpenAPI document by `npm run gen:api`; never edit the shapes here.
type Schemas = components['schemas']
export type Item = Schemas['ItemDto']
export type ItemInput = Schemas['ItemInput']
export type Profile = Schemas['ProfileDto']
export type ProfileUpdate = Schemas['ProfileUpdate']
export type Ingredient = Schemas['Ingredient']
export type Product = Schemas['ProductDto']
export type PantryItem = Schemas['PantryItem']
export type Suggestion = Schemas['SuggestionDto']
export type DailyCheckResult = Schemas['DailyCheckResultDto']
export type StorageLocation = Schemas['StorageLocation']
export type Urgency = Schemas['Urgency']
export type ItemStatus = Schemas['ItemStatus']
export type ConsumeOutcome = Schemas['ConsumeOutcome']
export type Language = NonNullable<Schemas['Language']>
export type DietType = Schemas['DietType']
export type Allergen = Schemas['Allergen']

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

// Looked up per call, not captured at start-up, so tests can stub fetch.
async function fetchOrOffline(request: Request): Promise<Response> {
  try {
    return await fetch(request)
  } catch {
    throw new Error(strings.errors.offline)
  }
}

const client = createClient<paths>({
  // Request needs an absolute URL, and tests run without a page to resolve against.
  baseUrl: globalThis.location?.origin ?? 'http://localhost',
  // X-EatUp: the backend rejects writes without it, so other websites can't send them (see Endpoints.cs).
  headers: { 'X-EatUp': '1' },
  fetch: fetchOrOffline,
})

async function unwrap<T>(call: Promise<{ data?: T; error?: unknown; response: Response }>): Promise<T> {
  const { data, error, response } = await call
  if (!response.ok) throw new ApiError(problemMessage(error as Problem, response.status), response.status)
  return data as T
}

const itemPath = (id: number) => ({ params: { path: { id } } })

export const api = {
  items: (status: ItemStatus = 'Active') => unwrap(client.GET('/api/items', { params: { query: { status } } })),
  item: (id: number) => unwrap(client.GET('/api/items/{id}', itemPath(id))),
  createItem: (input: ItemInput) => unwrap(client.POST('/api/items', { body: input })),
  updateItem: (id: number, input: ItemInput) => unwrap(client.PUT('/api/items/{id}', { ...itemPath(id), body: input })),
  deleteItem: (id: number) => unwrap(client.DELETE('/api/items/{id}', itemPath(id))),
  consume: (id: number, outcome: ConsumeOutcome, amount?: number) =>
    unwrap(client.POST('/api/items/{id}/consume', { ...itemPath(id), body: { outcome, amount: amount ?? null } })),

  profile: () => unwrap(client.GET('/api/profile')),
  /** Only the given fields are changed. */
  updateProfile: (changes: ProfileUpdate) => unwrap(client.PATCH('/api/profile', { body: changes })),

  ingredients: (search: string) => unwrap(client.GET('/api/ingredients', { params: { query: { search } } })),
  product: (barcode: string) => unwrap(client.GET('/api/products/{barcode}', { params: { path: { barcode } } })),
  suggestions: () => unwrap(client.GET('/api/suggestions')),

  vapidPublicKey: () => unwrap(client.GET('/api/push/vapid-public-key')).then((r) => r.publicKey),
  subscribe: (subscription: PushSubscriptionJSON) =>
    unwrap(client.POST('/api/push/subscriptions', { body: toSubscriptionInput(subscription) })),
  unsubscribe: (endpoint: string) => unwrap(client.DELETE('/api/push/subscriptions', { body: { endpoint } })),

  dev: {
    testPush: () => unwrap(client.POST('/api/dev/push/test')),
    runDailyCheck: () => unwrap(client.POST('/api/dev/run-daily-check')),
    resetDemoData: () => unwrap(client.POST('/api/dev/reset-demo-data')),
  },
}

function toSubscriptionInput(subscription: PushSubscriptionJSON): Schemas['PushSubscriptionInput'] {
  return {
    endpoint: subscription.endpoint ?? '',
    keys: { p256dh: subscription.keys?.p256dh ?? '', auth: subscription.keys?.auth ?? '' },
  }
}
