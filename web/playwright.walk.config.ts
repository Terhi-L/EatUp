import { defineConfig, devices } from '@playwright/test'
import base from './playwright.config'

// Same isolated API, web app and fresh database as the e2e tests, walked at phone and desktop size.
// Playwright empties outputDir at the start of every walk, so the screenshots are always from the latest one.
export default defineConfig({
  ...base,
  testMatch: '*.walk.ts',
  outputDir: 'flow-walk-results',
  projects: [
    { name: 'phone', use: { ...devices['Pixel 7'] } },
    { name: 'desktop', use: { ...devices['Desktop Chrome'], viewport: { width: 1440, height: 900 } } },
  ],
})
