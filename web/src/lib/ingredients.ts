import type { Ingredient } from '../api'

/** Typed text as the backend compares words: lower case, with spaces and hyphens as separators. */
export function typedWords(typed: string): string {
  return typed.toLowerCase().split(/[\s-]+/).filter(Boolean).join(' ')
}

/** The ingredient whose name or alias, in any language, is exactly the typed text, the way the backend compares words. */
export function findTypedIngredient(ingredients: Ingredient[] | undefined, typed: string): Ingredient | undefined {
  const words = typedWords(typed)
  if (!words) return undefined
  return ingredients?.find((i) => i.name.toLowerCase() === words || i.aliases.includes(words))
}
