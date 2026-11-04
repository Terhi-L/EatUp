import type { Ingredient, StorageLocation } from '../api'

/** Where the catalogue says to keep the item once opened, or null if it should stay put. */
export function locationAfterOpening(ingredient: Ingredient | undefined, current: StorageLocation): StorageLocation | null {
  const target = ingredient?.openedLocation
  if (!target || target === current) return null
  return target
}
