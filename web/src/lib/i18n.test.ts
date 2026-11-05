import { describe, expect, it } from 'vitest'
import type { Profile } from '../api'
import { detectLanguage, setLanguage, storedUnit, unitLabel } from '../strings'
import { en } from '../i18n/en'
import { nb } from '../i18n/nb'
import { fi } from '../i18n/fi'
import { profileChanges } from './profileSync'

describe('detectLanguage', () => {
  it('picks the first supported browser language', () => {
    expect(detectLanguage(['nb-NO', 'en'])).toBe('Norwegian')
    expect(detectLanguage(['nn'])).toBe('Norwegian')
    expect(detectLanguage(['de-DE', 'fi-FI'])).toBe('Finnish')
    expect(detectLanguage(['en-US', 'fi'])).toBe('English')
  })

  it('falls back to English', () => {
    expect(detectLanguage(['de-DE'])).toBe('English')
    expect(detectLanguage([])).toBe('English')
  })
})

describe('units', () => {
  it('show in the current language and store in English', () => {
    setLanguage('Finnish')
    expect(unitLabel('pcs')).toBe('kpl')
    expect(unitLabel('can')).toBe('purkkia')
    expect(unitLabel('g')).toBe('g')
    expect(storedUnit('kpl')).toBe('pcs')
    expect(storedUnit('Purkkia')).toBe('can')
    expect(storedUnit('dl')).toBe('dl')
    setLanguage('English')
  })

  it('store a unit typed in another language in English', () => {
    setLanguage('Norwegian')
    expect(storedUnit('kpl')).toBe('pcs')
    expect(storedUnit('purkkia')).toBe('can')
    setLanguage('English')
  })
})

describe('translations', () => {
  it('have the same validation errors as English', () => {
    expect(Object.keys(nb.apiErrors).sort()).toEqual(Object.keys(en.apiErrors).sort())
    expect(Object.keys(fi.apiErrors).sort()).toEqual(Object.keys(en.apiErrors).sort())
  })

  it('format dates in their own language', () => {
    const date = new Date(2026, 10, 3)
    expect(nb.inventory.today(date)).toContain('november')
    expect(fi.inventory.today(date)).toContain('marraskuuta')
  })
})

describe('profileChanges', () => {
  const profile: Profile = {
    diet: 'Omnivore', allergens: [], notificationTime: '09:00:00', timeZone: 'Europe/Oslo', notificationsEnabled: true, language: null,
  }

  it('reports the device language only until one is chosen', () => {
    expect(profileChanges(profile, 'Europe/Oslo', 'Norwegian')).toEqual({ language: 'Norwegian' })
    expect(profileChanges({ ...profile, language: 'Finnish' }, 'Europe/Oslo', 'Norwegian')).toEqual({})
  })

  it('keeps the time zone in sync with the browser', () => {
    expect(profileChanges({ ...profile, language: 'English' }, 'Europe/Helsinki', 'English')).toEqual({ timeZone: 'Europe/Helsinki' })
  })
})
