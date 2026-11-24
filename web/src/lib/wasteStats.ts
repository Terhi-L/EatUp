import type { WasteSummary } from '../api'

export type Encouragement = 'quietMonth' | 'nothingWasted' | 'lessThanLastMonth' | 'moreUsedThanWasted' | 'keepGoing'

/** The encouraging line that fits the month; the first that applies wins. */
export function encouragementFor({ thisMonth, lastMonth }: WasteSummary): Encouragement {
  if (thisMonth.used === 0 && thisMonth.discarded === 0) return 'quietMonth'
  if (thisMonth.discarded === 0) return 'nothingWasted'
  if (thisMonth.discarded < lastMonth.discarded) return 'lessThanLastMonth'
  if (thisMonth.used > thisMonth.discarded) return 'moreUsedThanWasted'
  return 'keepGoing'
}

/** A month's name in the given locale, e.g. (2026, 11, 'nb-NO') → "november". */
export function monthName(year: number, month: number, locale: string): string {
  return new Date(year, month - 1, 1).toLocaleDateString(locale, { month: 'long' })
}
