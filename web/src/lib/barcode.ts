import type { DecodeHintType } from '@zxing/library'
import { ApiError, api, type Product } from '../api'

// Phone photos are ~12 MP; barcode decoding is faster and just as reliable at this size.
const maxPhotoSide = 1600

export type ScanResult =
  | { kind: 'found'; product: Product }
  | { kind: 'noBarcode' }
  | { kind: 'unknownProduct' }
  | { kind: 'failed'; message: string }

export interface ScanDependencies {
  readBarcode: (photo: Blob) => Promise<string | null>
  findProduct: (barcode: string) => Promise<Product>
}

const browserDependencies: ScanDependencies = { readBarcode: readBarcodeFromPhoto, findProduct: api.product }

export async function scanProduct(photo: Blob, dependencies = browserDependencies): Promise<ScanResult> {
  const barcode = await dependencies.readBarcode(photo)
  if (!barcode) return { kind: 'noBarcode' }
  try {
    return { kind: 'found', product: await dependencies.findProduct(barcode) }
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return { kind: 'unknownProduct' }
    return { kind: 'failed', message: error instanceof Error ? error.message : String(error) }
  }
}

/** Scale factor that fits an image inside a square of `maxSide`, never enlarging it. */
export function scaleToFit(width: number, height: number, maxSide = maxPhotoSide): number {
  return Math.min(1, maxSide / Math.max(width, height))
}

async function readBarcodeFromPhoto(photo: Blob): Promise<string | null> {
  try {
    const canvas = await drawScaled(photo)
    const reader = await createProductBarcodeReader()
    return reader.decodeFromCanvas(canvas).getText()
  } catch {
    return null // no barcode found, or an image format the browser can't open
  }
}

async function drawScaled(photo: Blob): Promise<HTMLCanvasElement> {
  const bitmap = await createImageBitmap(photo)
  const scale = scaleToFit(bitmap.width, bitmap.height)
  const canvas = document.createElement('canvas')
  canvas.width = Math.round(bitmap.width * scale)
  canvas.height = Math.round(bitmap.height * scale)
  canvas.getContext('2d')!.drawImage(bitmap, 0, 0, canvas.width, canvas.height)
  bitmap.close()
  return canvas
}

// Loaded on first scan so the decoder isn't part of the start-up bundle.
async function createProductBarcodeReader() {
  const [{ BrowserMultiFormatReader }, { BarcodeFormat, DecodeHintType }] = await Promise.all([
    import('@zxing/browser'),
    import('@zxing/library'),
  ])
  const hints = new Map<DecodeHintType, unknown>([
    [DecodeHintType.POSSIBLE_FORMATS, [BarcodeFormat.EAN_13, BarcodeFormat.EAN_8, BarcodeFormat.UPC_A, BarcodeFormat.UPC_E]],
    [DecodeHintType.TRY_HARDER, true],
  ])
  return new BrowserMultiFormatReader(hints)
}
