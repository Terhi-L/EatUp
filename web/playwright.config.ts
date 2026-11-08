import { mkdtempSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { join } from 'node:path'
import { defineConfig, devices } from '@playwright/test'

const apiUrl = 'http://localhost:5081'
const webUrl = 'http://localhost:5174'
// A fresh database per run, so the test never sees or changes the data `npm run dev` uses.
const dataDirectory = mkdtempSync(join(tmpdir(), 'eatup-e2e-'))

export default defineConfig({
  testDir: 'e2e',
  testMatch: '*.e2e.ts',
  fullyParallel: false,
  workers: 1,
  reporter: 'list',
  use: { baseURL: webUrl, locale: 'en-GB', trace: 'retain-on-failure' },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: [
    {
      command: 'dotnet run --project ../backend/src/EatUp.Api --no-launch-profile',
      url: `${apiUrl}/api/health`,
      timeout: 180_000,
      reuseExistingServer: false,
      env: {
        ASPNETCORE_URLS: apiUrl,
        ASPNETCORE_ENVIRONMENT: 'Development',
        DataDirectory: dataDirectory,
        // Explicit, so an inherited connection string can't point the test's deletes at a real database.
        ConnectionStrings__EatUp: `Data Source=${join(dataDirectory, 'eatup.db')}`,
        Push__EnableDailyJob: 'false',
      },
    },
    {
      command: 'npx vite --port 5174',
      url: webUrl,
      timeout: 60_000,
      reuseExistingServer: false,
      env: { EATUP_API_URL: apiUrl },
    },
  ],
})
