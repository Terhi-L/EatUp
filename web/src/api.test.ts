import { afterEach, describe, expect, it, vi } from 'vitest'
import { api, type ItemInput } from './api'
import { setLanguage } from './strings'

function respondWith(status: number, problem: unknown) {
  vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify(problem), { status })))
}

describe('API errors', () => {
  afterEach(() => {
    setLanguage('English')
    vi.unstubAllGlobals()
  })

  it('translate validation errors by field', async () => {
    setLanguage('Norwegian')
    respondWith(400, { title: 'One or more validation errors occurred.', errors: { name: ['Name is required.'] } })
    await expect(api.createItem({ name: '' } as ItemInput)).rejects.toThrow('Navn må fylles ut.')
  })

  it('translate other errors by status instead of showing the English title', async () => {
    setLanguage('Finnish')
    respondWith(502, { title: "Couldn't reach Open Food Facts. Fill in the details yourself." })
    await expect(api.product('12345678')).rejects.toThrow('Tuotetietokantaan ei saatu yhteyttä. Täytä tiedot itse.')
  })

  it('fall back to the status code for anything unknown', async () => {
    setLanguage('Finnish')
    respondWith(500, { title: 'Internal Server Error' })
    await expect(api.items()).rejects.toThrow('Pyyntö epäonnistui (500)')
  })
})

describe('API writes', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('send the X-EatUp header the backend requires', async () => {
    const fetchSpy = vi.fn<(request: Request) => Promise<Response>>(async () => new Response(null, { status: 204 }))
    vi.stubGlobal('fetch', fetchSpy)
    await api.deleteItem(1)
    expect(fetchSpy.mock.calls[0][0].headers.get('X-EatUp')).toBe('1')
  })
})
