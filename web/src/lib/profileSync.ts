import type { Language, Profile } from '../api'

/**
 * What the device should write back to the profile on start-up: the browser's time zone, and the
 * device's language only until one is chosen. After that the profile's language wins, so every
 * device (and the daily notification) uses the same one.
 */
export function profileChanges(profile: Profile, browserTimeZone: string, deviceLanguage: Language): Partial<Profile> {
  return {
    ...(profile.timeZone !== browserTimeZone && { timeZone: browserTimeZone }),
    ...(profile.language === null && { language: deviceLanguage }),
  }
}
