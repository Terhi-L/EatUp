import { describe, expect, it } from 'vitest'
import type { WasteSummary } from '../api'
import { encouragementFor, monthName } from './wasteStats'

const summary = (used: number, discarded: number, lastDiscarded = 0): WasteSummary => ({
  thisMonth: { year: 2026, month: 12, used, discarded },
  lastMonth: { year: 2026, month: 11, used: 5, discarded: lastDiscarded },
  hasHistory: true,
})

describe('encouragementFor', () => {
  it('asks for the first entry in a month with nothing recorded yet', () => {
    expect(encouragementFor(summary(0, 0, 3))).toBe('quietMonth')
  })

  it('celebrates a month with nothing thrown out', () => {
    expect(encouragementFor(summary(4, 0))).toBe('nothingWasted')
  })

  it('notices less waste than last month', () => {
    expect(encouragementFor(summary(1, 2, 5))).toBe('lessThanLastMonth')
  })

  it('praises eating more than throwing out', () => {
    expect(encouragementFor(summary(6, 2, 2))).toBe('moreUsedThanWasted')
  })

  it('keeps going when more was thrown out than eaten', () => {
    expect(encouragementFor(summary(1, 3, 1))).toBe('keepGoing')
  })
})

describe('monthName', () => {
  it('names the month in the language of the screen', () => {
    expect(monthName(2026, 11, 'en-GB')).toBe('November')
    expect(monthName(2026, 11, 'nb-NO')).toBe('november')
    expect(monthName(2026, 11, 'fi-FI')).toBe('marraskuu')
  })
})
