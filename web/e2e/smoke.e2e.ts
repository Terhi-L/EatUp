import { expect, test, type APIRequestContext } from '@playwright/test'
import { en as text } from '../src/i18n/en'
import { addDays, localIsoDate } from '../src/lib/dates'

test.beforeEach(async ({ request }) => {
  await removeAllItems(request)
})

test('eggs about to expire are flagged, suggested for cooking and marked used', async ({ page }) => {
  await page.goto('/items/new')
  await page.getByLabel(text.form.name).fill('Eggs')
  await page.getByRole('button', { name: text.location.Fridge }).click()
  await page.getByLabel(text.form.expiry).fill(addDays(localIsoDate(), 2))
  await page.getByRole('button', { name: text.form.save }).click()

  const soonGroup = page.locator('section', { has: page.getByRole('heading', { name: text.urgency.Soon }) })
  await expect(soonGroup.getByText('Eggs', { exact: true })).toBeVisible()
  await expect(soonGroup.getByText(text.daysLeft(2), { exact: true })).toBeVisible()

  await page.getByRole('link', { name: text.nav.cook }).first().click()
  const usesEggs = `Eggs (${text.daysLeft(2).toLowerCase()})`
  await expect(page.locator('article').filter({ hasText: usesEggs }).first()).toBeVisible()

  await page.getByRole('link', { name: text.nav.food }).first().click()
  await page.getByRole('button', { name: text.inventory.markUsed('Eggs') }).click()
  await expect(page.getByText('Eggs', { exact: true })).toHaveCount(0)
})

async function removeAllItems(request: APIRequestContext) {
  const items: { id: number }[] = await (await request.get('/api/items')).json()
  for (const item of items) {
    const response = await request.delete(`/api/items/${item.id}`, { headers: { 'X-EatUp': '1' } })
    expect(response.ok()).toBe(true)
  }
}
