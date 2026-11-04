import { describe, expect, it } from 'vitest'
import { locationAfterOpening } from './storage'
import type { Ingredient } from '../api'

const ingredient = (openedLocation: Ingredient['openedLocation']): Ingredient =>
  ({ key: 'x', name: 'X', category: 'Test', allergens: [], shelfLifeDays: {}, openedLocation })

describe('locationAfterOpening', () => {
  it('moves an opened tin from the cupboard to the fridge', () => {
    expect(locationAfterOpening(ingredient('Fridge'), 'Cupboard')).toBe('Fridge')
  })

  it('keeps the item where it is when the catalogue has no rule', () => {
    expect(locationAfterOpening(ingredient(null), 'Cupboard')).toBeNull()
    expect(locationAfterOpening(undefined, 'Cupboard')).toBeNull()
  })

  it('does nothing when the item is already there', () => {
    expect(locationAfterOpening(ingredient('Fridge'), 'Fridge')).toBeNull()
  })
})
