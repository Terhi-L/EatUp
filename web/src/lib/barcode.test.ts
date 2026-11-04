import { describe, expect, it } from 'vitest'
import { ApiError, type Product } from '../api'
import { scaleToFit, scanProduct, type ScanDependencies } from './barcode'

const milk: Product = { name: 'Milk', productName: 'Tine Lettmelk', quantity: 1, unit: 'l' }
const photo = new Blob()

const scanWith = (dependencies: Partial<ScanDependencies>) =>
  scanProduct(photo, {
    readBarcode: async () => '7038010000737',
    findProduct: async () => milk,
    ...dependencies,
  })

describe('scanProduct', () => {
  it('returns the product behind the barcode', async () => {
    expect(await scanWith({})).toEqual({ kind: 'found', product: milk })
  })

  it('reports a photo without a readable barcode', async () => {
    expect(await scanWith({ readBarcode: async () => null })).toEqual({ kind: 'noBarcode' })
  })

  it('reports a barcode the product database does not know', async () => {
    const findProduct = async () => { throw new ApiError('Product not found.', 404) }
    expect(await scanWith({ findProduct })).toEqual({ kind: 'unknownProduct' })
  })

  it('passes other errors on', async () => {
    const findProduct = async () => { throw new ApiError("Couldn't reach Open Food Facts.", 502) }
    expect(await scanWith({ findProduct })).toEqual({ kind: 'failed', message: "Couldn't reach Open Food Facts." })
  })
})

describe('scaleToFit', () => {
  it('shrinks large photos to the longest side', () => {
    expect(scaleToFit(4000, 3000, 1600)).toBe(0.4)
    expect(scaleToFit(3000, 4000, 1600)).toBe(0.4)
  })

  it('never enlarges small photos', () => {
    expect(scaleToFit(800, 600, 1600)).toBe(1)
  })
})
