import { describe, expect, it } from 'vitest'
import { nameMatches } from './search'

describe('nameMatches', () => {
  it('finds the typed text anywhere in the name, ignoring case', () => {
    expect(nameMatches('Greek yoghurt', 'YOG')).toBe(true)
    expect(nameMatches('Greek yoghurt', ' yoghurt ')).toBe(true)
    expect(nameMatches('Milk', 'bread')).toBe(false)
  })

  it('matches Norwegian and Finnish letters regardless of case', () => {
    expect(nameMatches('Rømme', 'RØM')).toBe(true)
    expect(nameMatches('Blåbær', 'BLÅBÆR')).toBe(true)
    expect(nameMatches('Mämmi', 'MÄM')).toBe(true)
    expect(nameMatches('Öljy', 'öl')).toBe(true)
  })

  it('keeps å, ä and ö distinct from a and o', () => {
    expect(nameMatches('Blåbær', 'bla')).toBe(false)
    expect(nameMatches('Öljy', 'olj')).toBe(false)
  })

  it('matches letters typed or stored as a base letter plus accent', () => {
    expect(nameMatches('Blåbær'.normalize('NFD'), 'blå')).toBe(true)
    expect(nameMatches('Mämmi', 'Mämmi'.normalize('NFD'))).toBe(true)
  })

  it('shows everything for a blank search', () => {
    expect(nameMatches('Milk', '')).toBe(true)
    expect(nameMatches('Milk', '   ')).toBe(true)
  })
})
