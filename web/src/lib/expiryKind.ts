import type { ExpiryKind, Ingredient, Item } from '../api'

/** A kind the user tapped in the form, for the name the item had then. */
export type KindPick = { kind: ExpiryKind; name: string }

/** Whether two names are the same food name, ignoring case and surrounding spaces as the backend does. */
export function sameName(a: string, b: string): boolean {
  return a.trim().toLowerCase() === b.trim().toLowerCase()
}

/**
 * `sent` is a tap made for the current name, else the stored kind while the item keeps its name; otherwise null, so
 * the backend takes the default of the food it matches and renaming milk to chicken can't carry best before over.
 * `shown` previews that default from the form's own match; unknown food is use by.
 */
export function formExpiryKind(
  name: string, pick: KindPick | null, existing: Item | undefined, match: Ingredient | undefined,
): { shown: ExpiryKind; sent: ExpiryKind | null } {
  const sent = kindToKeep(name, pick, existing)
  return { shown: sent ?? match?.expiryKind ?? 'UseBy', sent }
}

function kindToKeep(name: string, pick: KindPick | null, existing: Item | undefined): ExpiryKind | null {
  if (pick && sameName(pick.name, name)) return pick.kind
  if (existing && sameName(existing.name, name)) return existing.expiryKind
  return null
}
