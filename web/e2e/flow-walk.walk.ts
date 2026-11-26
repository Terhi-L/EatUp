import { join } from 'node:path'
import { expect, test, type Locator, type Page } from '@playwright/test'
import type { ExpiryKind } from '../src/api'
import { en as text } from '../src/i18n/en'
import { addItem, removeAllItems } from './kitchen'

// Every inventory column has something in it. Chicken is use by, so it is to throw away; the yoghurt is
// best before and still worth a check, so it sits with the food to eat today.
const kitchen = [
  { name: 'Chicken', daysLeft: -1 },
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

test('food past its date: check the best-before food, throw out the rest', async ({ page, request }) => {
  await addItem(request, 'Kimchi', -1)
  const throwAway = group(page, text.urgency.Expired)
  const eatToday = group(page, text.urgency.Today)

  await test.step('use-by and unknown food are to throw away, best-before food to eat after a check', async () => {
    await page.goto('/')
    await expect(row(throwAway, 'Chicken')).toBeVisible()
    await expect(row(throwAway, 'Chicken').getByText(text.inventory.checkPackage)).toHaveCount(0)
    await expect(row(throwAway, 'Kimchi').getByText(text.inventory.checkPackage)).toBeVisible()
    await expect(row(eatToday, 'Yoghurt').getByText(text.pastBestBefore(-1))).toBeVisible()
    await expect(row(eatToday, 'Yoghurt').getByText(text.inventory.lookSmellTaste)).toBeVisible()
    await screenshot(page, 'past-date-groups')
  })

  await test.step('the add form takes the expiry kind from the catalogue', async () => {
    await visible(page.getByRole('link', { name: text.inventory.add })).click()
    await page.getByLabel(text.form.name).fill('Pasta')
    await expect(kindButton(page, 'BestBefore')).toHaveAttribute('aria-pressed', 'true')
    await screenshot(page, 'past-date-kind-from-catalogue')
    await page.getByLabel(text.form.name).fill('Chicken')
    await expect(kindButton(page, 'UseBy')).toHaveAttribute('aria-pressed', 'true')
  })

  await test.step('switching the unknown food to best before moves it to the food to eat after a check', async () => {
    await visible(page.getByRole('link', { name: text.nav.food })).click()
    await row(throwAway, 'Kimchi').getByRole('link').click()
    await expect(kindButton(page, 'UseBy')).toHaveAttribute('aria-pressed', 'true')
    await kindButton(page, 'BestBefore').click()
    await screenshot(page, 'past-date-kimchi-best-before')
    await page.getByRole('button', { name: text.form.save }).click()
    await expect(row(eatToday, 'Kimchi').getByText(text.inventory.lookSmellTaste)).toBeVisible()
    await screenshot(page, 'past-date-kimchi-to-check')
  })
})

function group(page: Page, heading: string): Locator {
  return page.locator('section', { has: page.getByRole('heading', { name: heading, exact: true }) })
}

function row(section: Locator, name: string): Locator {
  return section.getByRole('listitem').filter({ has: section.page().getByText(name, { exact: true }) })
}

function kindButton(page: Page, kind: ExpiryKind): Locator {
  return page.getByRole('button', { name: text.form.kind[kind], exact: true })
}

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
