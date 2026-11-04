import { useState } from 'react'
import type { Product } from '../../api'
import { scanProduct, type ScanResult } from '../../lib/barcode'
import { strings } from '../../strings'
import { BarcodeIcon } from '../../icons'

/** Takes a photo with the phone camera (or picks a file on desktop) and looks up the barcode in it. */
export function ScanBarcodeButton({ onFound }: { onFound: (product: Product) => void }) {
  const [scanning, setScanning] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  const scan = async (photo: File | undefined) => {
    if (!photo) return
    setScanning(true)
    setMessage(null)
    const result = await scanProduct(photo)
    setScanning(false)
    setMessage(scanMessage(result))
    if (result.kind === 'found') onFound(result.product)
  }

  return (
    <div>
      <label className={`btn-secondary w-full cursor-pointer focus-within:ring-4 focus-within:ring-accent/60 ${scanning ? 'opacity-50' : ''}`}>
        <BarcodeIcon />
        {scanning ? strings.scan.reading : strings.scan.button}
        <input type="file" accept="image/*" capture="environment" className="sr-only" disabled={scanning}
          onChange={(e) => {
            scan(e.target.files?.[0])
            e.target.value = '' // lets the same photo be picked again
          }} />
      </label>
      {message && <p role="status" className="mt-1 text-sm text-muted">{message}</p>}
    </div>
  )
}

function scanMessage(result: ScanResult): string {
  switch (result.kind) {
    case 'found': return strings.scan.found(result.product.productName ?? result.product.name)
    case 'noBarcode': return strings.scan.noBarcode
    case 'unknownProduct': return strings.scan.unknownProduct
    case 'failed': return strings.scan.failed
  }
}
