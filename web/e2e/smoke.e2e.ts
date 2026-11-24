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

test.describe('start-up time zone sync', () => {
  test.use({ timezoneId: 'Pacific/Kiritimati' })

  test('fetches the stats again once the profile has the device time zone', async ({ page, request }) => {
    const headers = { 'X-EatUp': '1' }
    const before: { timeZone: string } = await (await request.get('/api/profile')).json()
    // English is pinned so only the time zone changes; a first language save would refresh everything anyway.
    await request.patch('/api/profile', { data: { timeZone: 'UTC', language: 'English' }, headers })
    try {
      const events: string[] = []
      await page.route('**/api/profile', async (route) => {
        if (route.request().method() !== 'PATCH') return route.continue()
        await new Promise((resolve) => setTimeout(resolve, 1500))
        const response = await route.fetch()
        events.push('patch')
        await route.fulfill({ response })
      })
      await page.route('**/api/stats/waste', (route) => {
        events.push('stats')
        return route.continue()
      })

      await page.goto('/profile')

      await expect.poll(() => events.includes('patch') && events.lastIndexOf('stats') > events.indexOf('patch')).toBe(true)
    } finally {
      await request.patch('/api/profile', { data: { timeZone: before.timeZone }, headers })
    }
  })
})

test.describe('waste stats on Profile', () => {
  test('explain there is nothing yet instead of showing zeros', async ({ page }) => {
    // Other tests leave history in the shared database, so this one answers the stats call itself.
    const none = { year: 2026, month: 11, used: 0, discarded: 0 }
    await page.route('**/api/stats/waste', (route) => route.fulfill({ json: { thisMonth: none, lastMonth: none, hasHistory: false } }))
    await page.goto('/profile')

    await expect(page.getByText(text.waste.empty)).toBeVisible()
    await expect(page.getByText(text.waste.used(0))).toHaveCount(0)
  })

  test('refresh while the page stays open, so a new month shows up without reloading', async ({ page }) => {
    await page.clock.install()
    const month = { year: 2026, month: 11, used: 2, discarded: 0 }
    let calls = 0
    await page.route('**/api/stats/waste', (route) => {
      calls += 1
      return route.fulfill({ json: { thisMonth: month, lastMonth: month, hasHistory: true } })
    })
    await page.goto('/profile')
    await expect(page.getByText(text.waste.used(2))).toBeVisible()
    // The profile sync on start-up refetches everything once; count from after it settles.
    await page.waitForLoadState("networkidle")
    const callsBefore = calls

    await page.clock.fastForward('01:00:00')

    await expect.poll(() => calls).toBeGreaterThan(callsBefore)
  })

  test('count food eaten this month', async ({ page, request }) => {
    const id = await addItem(request, 'Yoghurt')
    const eaten = await request.post(`/api/items/${id}/consume`, { data: { outcome: 'Used' }, headers: { 'X-EatUp': '1' } })
    expect(eaten.ok()).toBe(true)

    await page.goto('/profile')

    await expect(page.getByRole('heading', { name: text.waste.title })).toBeVisible()
    await expect(page.getByText(/items? eaten/)).toBeVisible()
  })
})

test('using an item another tab already used explains why it disappeared', async ({ page, request }) => {
  const id = await addItem(request, 'Milk')
  const markUsed = page.getByRole('button', { name: text.inventory.markUsed('Milk') })
  await page.goto('/')
  await expect(markUsed).toBeVisible()

  const otherTab = await request.post(`/api/items/${id}/consume`, { data: { outcome: 'Used' }, headers: { 'X-EatUp': '1' } })
  expect(otherTab.ok()).toBe(true)
  await markUsed.click()

  await expect(page.getByRole('alert')).toHaveText(text.errors.byStatus[409]!)
  await expect(markUsed).toHaveCount(0)
})

test('a save that fails offline keeps the edits in the form', async ({ page, request }) => {
  const id = await addItem(request, 'Milk')
  await page.goto(`/items/${id}`)
  const name = page.getByLabel(text.form.name)
  await name.fill('Oat milk')

  await page.route('**/api/**', (route) => route.abort())
  await page.getByRole('button', { name: text.form.save }).click()
  await expect(page.getByText(text.errors.offline)).toBeVisible()
  // Nothing visible happens when the form survives, so give a background refetch time to fail.
  await page.waitForTimeout(2500)
  await expect(name).toHaveValue('Oat milk')

  await page.unroute('**/api/**')
  await page.getByRole('button', { name: text.form.save }).click()
  await expect(page.getByRole('button', { name: text.inventory.markUsed('Oat milk') })).toBeVisible()
})

async function addItem(request: APIRequestContext, name: string, daysLeft = 10): Promise<number> {
  const item = { name, ingredientKey: null, quantity: 1, unit: 'pcs', location: 'Fridge', expiryDate: addDays(localIsoDate(), daysLeft), openedDate: null }
  const response = await request.post('/api/items', { data: item, headers: { 'X-EatUp': '1' } })
  expect(response.ok()).toBe(true)
  return (await response.json()).id
}

async function removeAllItems(request: APIRequestContext) {
  const items: { id: number }[] = await (await request.get('/api/items')).json()
  for (const item of items) {
    const response = await request.delete(`/api/items/${item.id}`, { headers: { 'X-EatUp': '1' } })
    expect(response.ok()).toBe(true)
  }
}
