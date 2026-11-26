import { join } from 'node:path'
import { expect, test, type Locator, type Page } from '@playwright/test'
import { en as text } from '../src/i18n/en'
import { addItem, removeAllItems } from './kitchen'

// One item per urgency group, so every inventory column has something in it.
const kitchen = [
  { name: 'Yoghurt', daysLeft: -1 },
  { name: 'Bread', daysLeft: 0 },
  { name: 'Eggs', daysLeft: 2 },
  { name: 'Rice', daysLeft: 30 },
]

let shotNumber = 0

test.beforeEach(async ({ request }) => {
  shotNumber = 0
  await removeAllItems(request)
  for (const item of kitchen) await addItem(request, item.name, item.daysLeft)
})

test('the main loop: see the kitchen, add food, cook, eat it, see the month', async ({ page }) => {
  await test.step('the kitchen shows every urgency group', async () => {
    await page.goto('/')
    for (const urgency of Object.values(text.urgency)) await expect(page.getByRole('heading', { name: urgency, exact: true })).toBeVisible()
    await screenshot(page, 'kitchen')
  })

  await test.step('adding milk suggests its expiry date', async () => {
    await visible(page.getByRole('link', { name: text.inventory.add })).click()
    await page.getByLabel(text.form.name).fill('Milk')
    await page.getByRole('button', { name: text.location.Fridge }).click()
    await expect(page.getByText(text.form.expiryHint)).toBeVisible()
    await expect(page.getByLabel(text.form.expiry)).not.toHaveValue('')
    await screenshot(page, 'add-milk')
    await page.getByRole('button', { name: text.form.save }).click()
  })

  await test.step('the milk is in the kitchen', async () => {
    await expect(page.getByRole('button', { name: text.inventory.markUsed('Milk') })).toBeVisible()
    await screenshot(page, 'milk-added')
  })

  await test.step('cook now suggests a recipe with the eggs', async () => {
    await visible(page.getByRole('link', { name: text.nav.cook })).click()
    await expect(page.locator('article').filter({ hasText: `Eggs (${text.daysLeft(2).toLowerCase()})` }).first()).toBeVisible()
    await screenshot(page, 'cook-now')
  })

  // Phone and desktop share one database, so the count is checked against what was there before.
  const eatenBefore = await eatenThisMonth(page)

  await test.step('marking the eggs eaten takes them off the list', async () => {
    await visible(page.getByRole('link', { name: text.nav.food })).click()
    await page.getByRole('button', { name: text.inventory.markUsed('Eggs') }).click()
    await expect(page.getByRole('button', { name: text.inventory.markUsed('Eggs') })).toHaveCount(0)
    await screenshot(page, 'eggs-eaten')
  })

  await test.step('the profile counts what was eaten this month', async () => {
    await visible(page.getByRole('link', { name: text.nav.profile })).click()
    await expect(page.getByRole('heading', { name: text.waste.title })).toBeVisible()
    await expect(page.getByText(text.waste.used(eatenBefore + 1), { exact: true })).toBeVisible()
    await screenshot(page, 'profile-stats')
  })
})

// Phone and desktop render some links twice (bottom bar and sidebar, floating and header button).
function visible(locator: Locator): Locator {
  return locator.filter({ visible: true }).first()
}

async function eatenThisMonth(page: Page): Promise<number> {
  const stats: { thisMonth: { used: number } } = await (await page.request.get('/api/stats/waste')).json()
  return stats.thisMonth.used
}

async function screenshot(page: Page, step: string) {
  shotNumber += 1
  const project = test.info().project
  const file = `${String(shotNumber).padStart(2, '0')}-${step}.png`
  await page.screenshot({ path: join(project.outputDir, project.name, file), fullPage: true })
}
