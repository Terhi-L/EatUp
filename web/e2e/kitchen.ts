import { expect, type APIRequestContext } from '@playwright/test'
import { addDays, localIsoDate } from '../src/lib/dates'

export async function addItem(request: APIRequestContext, name: string, daysLeft = 10): Promise<number> {
  const item = { name, ingredientKey: null, quantity: 1, unit: 'pcs', location: 'Fridge', expiryDate: addDays(localIsoDate(), daysLeft), openedDate: null }
  const response = await request.post('/api/items', { data: item, headers: { 'X-EatUp': '1' } })
  expect(response.ok()).toBe(true)
  return (await response.json()).id
}

export async function removeAllItems(request: APIRequestContext) {
  const items: { id: number }[] = await (await request.get('/api/items')).json()
  for (const item of items) {
    const response = await request.delete(`/api/items/${item.id}`, { headers: { 'X-EatUp': '1' } })
    expect(response.ok()).toBe(true)
  }
}
