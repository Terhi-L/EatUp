import type { Ingredient } from '../api'

/** The ingredient whose name or alias, in any language, is exactly the typed text, the way the backend compares words. */
export function findTypedIngredient(ingredients: Ingredient[] | undefined, typed: string): Ingredient | undefined {
  const words = typed.toLowerCase().split(/[\s-]+/).filter(Boolean).join(' ')
  if (!words) return undefined
  return ingredients?.find((i) => i.name.toLowerCase() === words || i.aliases.includes(words))
}
