import type { Item } from '../api'

/**
 * Whether the form's date is a shelf-life estimate: a new item's untouched suggestion, or an existing item's estimate
 * until the user edits the date (even back to the same day) or confirms it matches the package. A new item without a
 * date is left to the backend, which estimates it anyway.
 */
export function sentExpiryEstimated(
  existing: Item | undefined, expiryDate: string, expiryTouched: boolean, dateConfirmed: boolean,
): boolean {
  if (dateConfirmed) return false
  if (existing) return existing.expiryEstimated && expiryDate === existing.expiryDate
  return !expiryTouched && expiryDate !== ''
}
