import { describe, expect, it } from 'vitest'
import type { Ingredient } from '../api'
import { findTypedIngredient } from './ingredients'

const chickpeas: Ingredient = {
  key: 'chickpeas', name: 'Kikerter', aliases: ['chickpeas', 'kikerter', 'kikherneet'],
  category: 'Dry goods', allergens: [], animal: 'None', shelfLifeDays: { Cupboard: 365 }, openedShelfLifeDays: null, openedLocation: 'Fridge',
  expiryKind: 'BestBefore', checkDaysPastBestBefore: { Cupboard: 180 },
}

describe('findTypedIngredient', () => {
  it('matches the shown name or an alias in another language', () => {
    expect(findTypedIngredient([chickpeas], 'Kikerter')).toBe(chickpeas)
    expect(findTypedIngredient([chickpeas], '  Chickpeas ')).toBe(chickpeas)
  })

  it('treats hyphens and repeated spaces as word breaks, like the backend', () => {
    const creamCheese = { ...chickpeas, key: 'cream-cheese', name: 'Cream cheese', aliases: ['cream cheese'] }

    expect(findTypedIngredient([creamCheese], 'Cream-cheese')).toBe(creamCheese)
    expect(findTypedIngredient([creamCheese], 'cream   cheese')).toBe(creamCheese)
  })

  it('ignores partial or empty text', () => {
    expect(findTypedIngredient([chickpeas], 'chick')).toBeUndefined()
    expect(findTypedIngredient([chickpeas], '')).toBeUndefined()
  })
})
