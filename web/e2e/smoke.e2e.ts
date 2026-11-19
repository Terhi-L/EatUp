import { expect, test, type APIRequestContext, type Page } from '@playwright/test'
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

test.describe('inventory search', () => {
  const names = ['Rømme', 'Mämmi', 'Milk']
  const row = (page: Page, name: string) => page.getByRole('button', { name: text.inventory.markUsed(name) })

  test.beforeEach(async ({ request }) => {
    for (const name of names) await addItem(request, name)
  })

  test('the phone header search filters by name in any case and closes back to the full list', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 })
    await page.goto('/')
    await page.getByRole('button', { name: text.inventory.search }).click()
    const search = page.getByRole('searchbox', { name: text.inventory.search })

    await search.fill('RØM')
    await expect(row(page, 'Rømme')).toBeVisible()
    await expect(row(page, 'Milk')).toHaveCount(0)

    await search.fill('kale')
    await expect(page.getByText(text.inventory.noResults('kale'))).toBeVisible()

    await page.getByRole('button', { name: text.inventory.closeSearch }).click()
    for (const name of names) await expect(row(page, name)).toBeVisible()
  })

  test('the desktop search box filters as you type and clears back to the full list', async ({ page }) => {
    await page.goto('/')
    const search = page.getByRole('searchbox', { name: text.inventory.search })

    await search.pressSequentially('mäm')
    await expect(row(page, 'Mämmi')).toBeVisible()
    await expect(row(page, 'Rømme')).toHaveCount(0)

    await page.getByRole('button', { name: text.inventory.clearSearch }).click()
    for (const name of names) await expect(row(page, name)).toBeVisible()
  })
})

test.describe('Tonight, maybe? card on desktop', () => {
  const card = (page: Page) => page.getByRole('region', { name: text.inventory.tonight })

  test('shows the top suggestion and links to Cook now', async ({ page, request }) => {
    await addItem(request, 'Eggs', 2)
    await page.goto('/')

    await expect(card(page).getByText('Eggs')).toBeVisible()
    await card(page).getByRole('link', { name: text.inventory.seeRecipe }).click()
    await expect(page).toHaveURL('/cook')
  })

  test('is hidden when nothing needs cooking', async ({ page, request }) => {
    await addItem(request, 'Milk', 10)
    const suggestions = page.waitForResponse('**/api/suggestions')
    await page.goto('/')
    await suggestions

    await expect(page.getByRole('button', { name: text.inventory.markUsed('Milk') })).toBeVisible()
    await expect(card(page)).toHaveCount(0)
  })
})

async function addItem(request: APIRequestContext, name: string, daysLeft = 10) {
  const item = { name, ingredientKey: null, quantity: 1, unit: 'pcs', location: 'Fridge', expiryDate: addDays(localIsoDate(), daysLeft), openedDate: null }
  const response = await request.post('/api/items', { data: item, headers: { 'X-EatUp': '1' } })
  expect(response.ok()).toBe(true)
}

async function removeAllItems(request: APIRequestContext) {
  const items: { id: number }[] = await (await request.get('/api/items')).json()
  for (const item of items) {
    const response = await request.delete(`/api/items/${item.id}`, { headers: { 'X-EatUp': '1' } })
    expect(response.ok()).toBe(true)
  }
}
