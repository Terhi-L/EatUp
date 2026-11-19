import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { chromium } from '@playwright/test'

const publicDir = new URL('../web/public/', import.meta.url)
const favicon = readFileSync(new URL('favicon.svg', publicDir), 'utf8')
const faviconUrl = `data:image/svg+xml;base64,${Buffer.from(favicon).toString('base64')}`
const tileColor = favicon.match(/<rect[^>]*fill="([^"]+)"/)[1]

// Full-bleed icons paint the tile colour behind the favicon, which hides its rounded corners:
// iOS turns transparency black, and maskable icons must fill the whole square.
// Maskable content must stay inside the central circle of 80% diameter.
const icons = [
  { file: 'icon-192.png', size: 192, fullBleed: false, artworkScale: 1 },
  { file: 'icon-512.png', size: 512, fullBleed: false, artworkScale: 1 },
  { file: 'icon-maskable-512.png', size: 512, fullBleed: true, artworkScale: 0.8 },
  { file: 'apple-touch-icon.png', size: 180, fullBleed: true, artworkScale: 1 },
]

function iconPage({ size, fullBleed, artworkScale }) {
  const background = fullBleed ? tileColor : 'transparent'
  const artworkSize = size * artworkScale
  return `<body style="margin:0;width:${size}px;height:${size}px;display:grid;place-items:center;background:${background}">
    <img src="${faviconUrl}" width="${artworkSize}" height="${artworkSize}">
  </body>`
}

const browser = await chromium.launch()
try {
  const page = await browser.newPage()
  for (const icon of icons) {
    await page.setViewportSize({ width: icon.size, height: icon.size })
    await page.setContent(iconPage(icon))
    await page.screenshot({ path: fileURLToPath(new URL(icon.file, publicDir)), omitBackground: !icon.fullBleed })
  }
} finally {
  await browser.close()
}
