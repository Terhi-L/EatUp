import { useSyncExternalStore } from 'react'
import type { Language } from './api'
import { en, type Strings } from './i18n/en'
import { nb } from './i18n/nb'
import { fi } from './i18n/fi'

export const languages: Language[] = ['English', 'Norwegian', 'Finnish']

/** Each language named in itself, so people can find theirs. */
export const languageNames: Record<Language, string> = { English: 'English', Norwegian: 'Norsk', Finnish: 'Suomi' }

const dictionaries: Record<Language, Strings> = { English: en, Norwegian: nb, Finnish: fi }
const storageKey = 'eatup.language'
const listeners = new Set<() => void>()

export let language: Language = savedLanguage() ?? detectLanguage(globalThis.navigator?.languages ?? [])

/**
 * All user-facing copy in the current language. A live binding: read it while rendering,
 * and the app re-renders from the root (see useLanguage) when the language changes.
 */
export let strings: Strings = dictionaries[language]
showLanguageOnPage()

export function setLanguage(next: Language) {
  if (next === language) return
  language = next
  strings = dictionaries[next]
  saveLanguage(next)
  showLanguageOnPage()
  listeners.forEach((notify) => notify())
}

/** The current language; components using it re-render when it changes. */
export function useLanguage(): Language {
  return useSyncExternalStore(subscribe, () => language)
}

/** Best match for the browser's preferred languages, e.g. ["nb-NO", "en"] → Norwegian. */
export function detectLanguage(preferred: readonly string[]): Language {
  for (const tag of preferred) {
    const primary = tag.toLowerCase().split('-')[0]
    if (['nb', 'nn', 'no'].includes(primary)) return 'Norwegian'
    if (primary === 'fi') return 'Finnish'
    if (primary === 'en') return 'English'
  }
  return 'English'
}

type UnitKey = keyof Strings['form']['units']

/** Units are stored in English (what the backend and scans use) but shown in the current language. */
export function unitLabel(unit: string): string {
  return strings.form.units[unit as UnitKey] ?? unit
}

/** The reverse of unitLabel, so a unit typed in any supported language is stored the same way. */
export function storedUnit(label: string): string {
  const typed = label.trim()
  const units = Object.values(dictionaries).flatMap((d) => Object.entries(d.form.units))
  return units.find(([, shown]) => shown === typed.toLowerCase())?.[0] ?? typed
}

export function localeOf(l: Language): string {
  return dictionaries[l].locale
}

function subscribe(notify: () => void) {
  listeners.add(notify)
  return () => listeners.delete(notify)
}

function showLanguageOnPage() {
  if (typeof document !== 'undefined') document.documentElement.lang = strings.locale
}

// Storage can be missing or blocked (private mode, tests); the language then comes from the browser or profile.
function savedLanguage(): Language | null {
  try {
    const saved = globalThis.localStorage?.getItem(storageKey)
    return languages.find((l) => l === saved) ?? null
  } catch {
    return null
  }
}

function saveLanguage(l: Language) {
  try {
    globalThis.localStorage?.setItem(storageKey, l)
  } catch {
    // Not remembered on this device; the profile still has it.
  }
}
