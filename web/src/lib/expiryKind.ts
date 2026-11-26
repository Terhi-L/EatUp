import type { ExpiryKind, Ingredient, Item } from '../api'

/** A kind the user tapped in the form, for the name the item had then. */
export type KindPick = { kind: ExpiryKind; name: string }

/**
 * `sent` is a tap made for the current name, else the stored kind while the item keeps its name; otherwise null, so
 * the backend takes the default of the food it matches and renaming milk to chicken can't carry best before over.
 * `shown` previews that default from the form's own match; unknown food is use by.
 */
export function formExpiryKind(
  name: string, pick: KindPick | null, existing: Item | undefined, match: Ingredient | undefined,
): { shown: ExpiryKind; sent: ExpiryKind | null } {
  const typed = name.trim()
  const sent = pick?.name === typed ? pick.kind : existing?.name === typed ? existing.expiryKind : null
  return { shown: sent ?? match?.expiryKind ?? 'UseBy', sent }
}
