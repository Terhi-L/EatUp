import { describe, expect, it } from 'vitest'
import { addDays, localIsoDate } from './dates'
import { strings } from '../strings'
import { urlBase64ToUint8Array } from '../features/push/push'

describe('dates', () => {
  it('formats the local date', () => {
    expect(localIsoDate(new Date(2026, 0, 5))).toBe('2026-01-05')
  })

  it('adds days across month and year ends', () => {
    expect(addDays('2026-12-30', 3)).toBe('2027-01-02')
    expect(addDays('2026-03-01', -1)).toBe('2026-02-28')
  })
})

describe('daysLeft label', () => {
  it.each([
    [-3, 'Expired 3 days ago'],
    [-1, 'Expired yesterday'],
    [0, 'Today'],
    [1, 'Tomorrow'],
    [5, 'In 5 days'],
  ])('%i → %s', (days, label) => {
    expect(strings.daysLeft(days)).toBe(label)
  })
})

describe('urlBase64ToUint8Array', () => {
  it('decodes url-safe base64 without padding', () => {
    expect(Array.from(urlBase64ToUint8Array('-_8'))).toEqual([0xfb, 0xff])
  })
})
