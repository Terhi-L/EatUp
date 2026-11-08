# Getting started

Everything in EatUp runs on your own machine. No hosting, no paid services and no credit card needed (see [PLAN.md § 0](PLAN.md#0-guiding-principle-local-first-zero-cost)).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (current LTS) and npm
- Chrome or Edge (best support for installing PWAs and Web Push on desktop)
- Optional: [`cloudflared`](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/downloads/), for testing on a real phone

## Run it

```sh
npm install
npm run dev
```

This starts the C# backend (http://localhost:5080) and the web app (http://localhost:5173) together. On first run the backend:

- creates the SQLite database in `backend/data/` (git-ignored) and applies any pending EF Core migrations
- fills it with demo food: **avocado expires today**, **eggs tomorrow**, opened milk, spinach and bread in 2 days, and so on. Dates are relative to the current day, so there's always something to cook.
- generates Web Push (VAPID) keys into `backend/data/vapid.json`

Then open **http://localhost:5173**. There's no login in the MVP: it's a single local user.

To start from scratch, stop the app and delete `backend/data/eatup.db`. Keep `vapid.json`, or installed apps must turn notifications on again.

Changing the data model? Add a migration instead of deleting the database; the backend applies it on the next start:

```bash
dotnet ef migrations add <DescriptiveName> --project backend/src/EatUp.Api --output-dir Data/Migrations
```

This needs the EF tool once: `dotnet tool install -g dotnet-ef`.

## Test the app as a phone app

`localhost` counts as a secure origin, so **installing the PWA and receiving notifications both work without HTTPS**.

1. Open DevTools → device mode to see the mobile layout.
2. Install the app: click the install icon in the address bar (Chrome/Edge).
3. Open **Profile** and tap **Turn on notifications**.

## Test notifications without waiting for the morning

The real daily reminder fires at the time set on the Profile screen (default 09:00). The backend checks every 5 minutes and sends at most one a day. In development, the Profile screen has buttons so you don't have to wait:

| Button | What it does |
|---|---|
| **Send test notification** | Checks that push works at all |
| **Run daily check now** | Runs the real "what's about to expire?" logic immediately, ignoring the time and the once-a-day limit. With demo data you get *"Avocado expires today (+4 more)"* with a cook idea |
| **Reset demo data** | Restores the demo food with dates relative to today |

Tapping a notification opens the **Cook now** screen. These buttons and their `/api/dev/*` endpoints exist only in Development.

If notifications don't show up, check that Windows/macOS notifications are allowed for your browser, and that the site isn't blocked in the browser's notification settings.

## Optional: try it on a real phone

```sh
cloudflared tunnel --url http://localhost:5173
```

This prints a temporary `https://….trycloudflare.com` address you can open on your phone. It's free and needs no account. Caveats:

- The address changes every run, so you'll need to reinstall the app on the phone each time.
- On iPhone (iOS 16.4+), notifications only work after **Share → Add to Home Screen**, opening EatUp from the home screen.
- Anyone with the address can use your local EatUp, so stop the tunnel when you're done.

## Other commands

| Command | What it does |
|---|---|
| `npm test` | Runs backend (xUnit) and frontend (Vitest) tests |
| `npm run build` | Builds the backend and the production web app |
| `npm run typecheck` | Type-checks the web app |
| `npm run e2e` | Runs the Playwright smoke test: add eggs, see them flagged and suggested, mark them used. It starts its own API (port 5081) and web app (port 5174) on an empty temporary database, so your data is untouched. Install the browser once first: `npx -w web playwright install chromium` |

## Where things live

| Path | What |
|---|---|
| `backend/src/EatUp.Api/Domain/` | Pure rules: expiry, suggestions, daily notification decision |
| `backend/src/EatUp.Api/Seed/catalog.json` | Ingredients (shelf life, allergens, diet flags) and recipes |
| `backend/src/EatUp.Api/Endpoints.cs` | All HTTP endpoints |
| `backend/tests/EatUp.Tests/` | Backend tests |
| `web/src/features/` | Screens: inventory, cook now, profile, push |
| `web/src/sw.ts` | Service worker: offline app shell, push, notification click |
| `web/src/strings.ts` | All UI text |

See [PLAN.md](PLAN.md) for the roadmap and `docs/decisions.md` for decisions made along the way.
