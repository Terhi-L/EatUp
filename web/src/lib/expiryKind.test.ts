import { describe, expect, it } from 'vitest'
import type { ExpiryKind, Ingredient, Item } from '../api'
import { formExpiryKind } from './expiryKind'

const ingredient = (key: string, expiryKind: ExpiryKind): Ingredient => ({
  key, name: key, aliases: [key], category: 'Test', allergens: [], animal: 'None',
  shelfLifeDays: {}, openedShelfLifeDays: null, expiryKind,
})
const milk = ingredient('milk', 'BestBefore')
const chicken = ingredient('chicken', 'UseBy')

const storedMilk = { name: 'Milk', ingredientKey: 'milk', expiryKind: 'BestBefore' } as Item

describe('formExpiryKind', () => {
  it('leaves a renamed item to the backend’s default for its new food', () => {
    expect(formExpiryKind('Chicken', null, storedMilk, chicken)).toEqual({ shown: 'UseBy', sent: null })
    expect(formExpiryKind('Kimchi', null, storedMilk, undefined)).toEqual({ shown: 'UseBy', sent: null })
  })

  it('keeps the stored kind, a switch included, while the item keeps its name', () => {
    const switchedMilk = { ...storedMilk, expiryKind: 'UseBy' } as Item

    expect(formExpiryKind('Milk', null, switchedMilk, milk)).toEqual({ shown: 'UseBy', sent: 'UseBy' })
    expect(formExpiryKind('Milk ', null, switchedMilk, milk)).toEqual({ shown: 'UseBy', sent: 'UseBy' })
    expect(formExpiryKind('milk', null, switchedMilk, milk)).toEqual({ shown: 'UseBy', sent: 'UseBy' })
  })

  it('treats hyphens and extra spaces as the same name', () => {
    const switchedCreamCheese = { name: 'Cream cheese', ingredientKey: 'cream-cheese', expiryKind: 'UseBy' } as Item

    expect(formExpiryKind('Cream-cheese', null, switchedCreamCheese, undefined).sent).toBe('UseBy')
    expect(formExpiryKind('Cream  cheese', null, switchedCreamCheese, undefined).sent).toBe('UseBy')
    expect(formExpiryKind('Cream-cheese', { kind: 'UseBy', name: 'Cream cheese' }, undefined, undefined).sent).toBe('UseBy')
  })

  it('sends a tap for the name it was made for', () => {
    expect(formExpiryKind('Chicken', { kind: 'BestBefore', name: 'Chicken' }, storedMilk, chicken))
      .toEqual({ shown: 'BestBefore', sent: 'BestBefore' })
  })

  it('drops a tap once the name changes', () => {
    expect(formExpiryKind('Chicken', { kind: 'BestBefore', name: 'Milk' }, undefined, chicken)).toEqual({ shown: 'UseBy', sent: null })
  })

  it('previews a new item’s kind from the form’s match and leaves the choice to the backend', () => {
    expect(formExpiryKind('Milk', null, undefined, milk)).toEqual({ shown: 'BestBefore', sent: null })
    expect(formExpiryKind('2 eggs', null, undefined, undefined)).toEqual({ shown: 'UseBy', sent: null })
  })
})
