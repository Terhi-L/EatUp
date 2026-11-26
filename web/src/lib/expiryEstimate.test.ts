import { describe, expect, it } from 'vitest'
import type { Item } from '../api'
import { sentExpiryEstimated } from './expiryEstimate'

const estimatedMilk = { expiryDate: '2026-12-03', expiryEstimated: true } as Item
const enteredMilk = { expiryDate: '2026-12-03', expiryEstimated: false } as Item

describe('sentExpiryEstimated', () => {
  it('counts a new item’s untouched suggestion as estimated, and a typed date as entered', () => {
    expect(sentExpiryEstimated(undefined, '2026-12-03', false, false)).toBe(true)
    expect(sentExpiryEstimated(undefined, '2026-12-03', true, false)).toBe(false)
  })

  it('leaves a new item without a date to the backend, which estimates it anyway', () => {
    expect(sentExpiryEstimated(undefined, '', false, false)).toBe(false)
  })

  it('keeps a stored estimate until the date changes', () => {
    expect(sentExpiryEstimated(estimatedMilk, '2026-12-03', true, false)).toBe(true)
    expect(sentExpiryEstimated(estimatedMilk, '2026-12-05', true, false)).toBe(false)
  })

  it('counts a date the user edited or confirmed as entered, even when it ends up the same', () => {
    expect(sentExpiryEstimated(estimatedMilk, '2026-12-03', true, true)).toBe(false)
    expect(sentExpiryEstimated(undefined, '2026-12-03', true, true)).toBe(false)
  })

  it('never turns an entered date into an estimate', () => {
    expect(sentExpiryEstimated(enteredMilk, '2026-12-03', true, false)).toBe(false)
  })
})
