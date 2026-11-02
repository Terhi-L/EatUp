# EatUp — Implementation Plan

> This plan is written for a Claude coding agent. Work through the phases in order.
> Each phase ends with acceptance criteria; do not start the next phase until they pass.
> When a decision isn't covered here, pick the simplest option that keeps the next phase possible, and note it in `docs/decisions.md`.
> **Status (2026-11-02):** a short MVP without login is built. See `docs/decisions.md` for how it differs from the phases below.

## 0. Guiding principle: local-first, zero cost

This is a hobby project that should cost nothing.

- **Everything runs on the developer's machine.** There is no hosting, deployment or server setup in this plan, in the MVP or after it.
- If a feature truly needs something outside the machine, it must be **free and must not require registering a payment card**. Prefer tools with no account at all.
- If a free hosted service is used later (e.g. a free tier that wipes data on deploy), that's acceptable. The app must keep working fully locally regardless.
- Never add a dependency on a paid service. If a phase would need one, stop and ask the product owner.

## 1. Product summary

EatUp helps people **reduce food waste** by tracking fresh food and its expiry dates, and nudging them to cook with what is about to go off.

- Users register food items (milk, cream cheese, eggs, avocado…) with an expiry date and a storage location (fridge, freezer, pantry, counter).
- When an item expires **today, in 1 day or in 2 days**, the user gets a notification with a concrete suggestion: *"Your eggs expire tomorrow — whip up an omelette!"*
- Suggestions respect the user's **diet** (omnivore, flexitarian, pescatarian, vegetarian, vegan) and **allergies**, and lean plant-forward: the less meat, the better.
- Later: weekly meal planning with health feedback (too much fast food, missing protein, no omega-3/fish, etc.) and camera-based item entry.

The product is an **installable Progressive Web App (PWA)**: one web codebase, no app stores, installable to the home screen.

## 2. Scope

### MVP (phases 0–4)
- ~~Account sign-up / login~~: **moved out of the MVP** (single local user). Email + password and optional Google sign-in come later, as described in phases 1 and 3.
- Manual add / edit / delete of items: name, category, quantity + unit, storage location, expiry date, optional "opened" date
- Inventory list sorted by expiry, with colour-coded urgency (expired / today / ≤2 days / later)
- Mark item as **used**, **thrown away** or **partially used** (this is the waste data we'll want later)
- Diet preference + allergy profile
- Recipe suggestions for items expiring within 2 days, filtered by diet and allergies
- Installable PWA (manifest, icons, service worker, install guidance for iPhone)
- Daily **Web Push** notification for items expiring in 0–2 days, with a suggested dish, sent by the backend
- All of it runs locally with one command

### Post-MVP (phases 5–8)
- Barcode scanning (Open Food Facts lookup for name/category/allergens)
- Camera: photograph product / expiry label, extract name + date
- Weekly meal planner with health and plant-forward analysis
- Waste statistics ("you saved 14 items this month")
- Shared household inventory
- Offline support (add/edit items offline, sync later)

## 3. Tech stack

| Layer | Choice | Why |
|---|---|---|
| Frontend | **React + TypeScript + Vite** | Fast, standard, TypeScript-first |
| PWA | `vite-plugin-pwa` with the **`injectManifest`** strategy (custom service worker in `src/sw.ts`, Workbox for precaching) | A custom service worker is needed to handle `push` and `notificationclick` events |
| Routing | React Router | Standard |
| Data fetching | TanStack Query | Caching, refetch, optimistic updates |
| Forms / validation | react-hook-form + zod | Typed forms; zod schemas mirror API contracts |
| Styling | Tailwind CSS | Quick mobile-first UI without a component library dependency |
| Backend | **ASP.NET Core (.NET 10, C#) Minimal APIs** | User preference; minimal APIs keep the surface small |
| Persistence | **EF Core + SQLite** (a local file, `backend/data/eatup.db`, git-ignored) | No database server to install |
| Auth | ASP.NET Core Identity (`MapIdentityApi`) using **HttpOnly, SameSite=Strict cookies** (`/login?useCookies=true`) for email + password. **Sign in with Google** (optional): the frontend uses Google Identity Services (`@react-oauth/google`) to get a Google ID token, posts it to the backend, which validates it (`Google.Apis.Auth` → `GoogleJsonWebSignature.ValidateAsync`), finds/creates the Identity user and signs them in with the same cookie | Browser and API share one origin (via the Vite proxy), so cookies are simpler and safer than storing tokens in the browser. A Google OAuth client ID is free and needs no card |
| Web Push | `WebPush` NuGet package (VAPID keys generated locally) on the backend; Push API + service worker on the frontend | Free, no account needed; works on `localhost` without HTTPS |
| Scheduled work | `BackgroundService` in the API process | Runs while the backend runs; no extra infrastructure |
| Dev runner | Root `package.json` with `npm run dev` starting backend + frontend via `concurrently` | One command to run everything |
| Hosting | **None.** Local only (see section 0) | Zero cost |
| Language | **English only** for now | Keep all UI strings in one module (`src/strings.ts`) so i18n can be added later without hunting through components |
| API contract | OpenAPI (built-in `Microsoft.AspNetCore.OpenApi`) → generated TS types (`openapi-typescript` + `openapi-fetch`) | One source of truth for types across C# and TS |
| Backend tests | xUnit + `WebApplicationFactory` + SQLite in-memory | Real HTTP pipeline in tests |
| Frontend tests | Vitest + React Testing Library; Playwright for a smoke e2e test | Native to Vite |

## 4. Repository layout

```
EatUp/
├── PLAN.md
├── CLAUDE.md                     # agent conventions (create in phase 0)
├── package.json                  # root scripts: dev, test, lint, gen:api
├── docs/decisions.md             # short ADR-style log
├── backend/
│   ├── EatUp.sln
│   ├── data/                     # local SQLite file (git-ignored)
│   ├── src/EatUp.Api/            # endpoints, DI, auth, OpenAPI, push sender, background job
│   ├── src/EatUp.Core/           # domain: entities, expiry rules, suggestion engine, notification rules (no EF/HTTP deps)
│   ├── src/EatUp.Infrastructure/ # EF Core DbContext, migrations, seed data
│   └── tests/EatUp.Tests/
├── web/                          # React + Vite PWA
│   ├── public/                   # icons, manifest assets
│   ├── src/sw.ts                 # service worker: precache, push, notificationclick
│   ├── src/api/                  # generated types + client + query hooks
│   ├── src/features/auth/
│   ├── src/features/inventory/
│   ├── src/features/suggestions/
│   ├── src/features/profile/
│   ├── src/features/push/        # permission flow, subscribe/unsubscribe, iOS install guidance
│   └── src/strings.ts
└── data/recipes.seed.json        # seed recipe catalogue
```

### Local development
- `npm install` then `npm run dev` from the repo root: starts the API and Vite, applies EF migrations, creates the SQLite file and seeds demo data on first run.
- Open `http://localhost:5173` in Chrome or Edge. Vite proxies `/api` to the backend, so the browser sees a single origin and cookies work.
- `localhost` counts as a secure context: **installing the PWA and Web Push both work without HTTPS**. Use DevTools device mode to check the mobile layout.
- Secrets (VAPID private key, optional Google client ID) go in .NET user secrets. On first run, if no VAPID keys exist, generate them and print the command to store them.
- **Optional, testing on a real phone:** `cloudflared tunnel --url http://localhost:5173` gives a temporary HTTPS address. It's free and needs no account. Caveats: the address changes every run (reinstall the app each time), Google login won't work there (address not on Google's allowed list) but email login does, and on iPhone notifications only work once the app is added to the home screen (iOS 16.4+).

## 5. Domain model

```
User (Identity)
  └─ UserProfile: DietType, Allergens[], NotificationTime (default 09:00), TimeZone (IANA, from the browser),
                  NotificationsEnabled, LastNotifiedOn (date, in the user's time zone)

PushSubscription
  Id, UserId, Endpoint (unique), P256dh, Auth, UserAgent, CreatedAt, LastSuccessAt?
  (one user can have several devices/browsers)

FoodItem
  Id, UserId, Name, Category, Quantity, Unit,
  StorageLocation (Fridge | Freezer | Pantry | Counter),
  ExpiryDate (date only), ExpiryKind (UseBy | BestBefore | Estimated),
  OpenedDate?, Status (Active | Used | Discarded), StatusChangedAt, CreatedAt

Ingredient (canonical catalogue: "egg", "milk", "avocado"…)
  Id, Name, Aliases[], Category, Allergens[], DietFlags (IsMeat, IsFish, IsDairy, IsEgg, IsAnimal),
  DefaultShelfLifeDays per StorageLocation, OpenedShelfLifeDays?

Recipe
  Id, Title, Description, Steps, PrepMinutes, Tags,
  RecipeIngredients[] (IngredientId, Optional?)
  -> derived: SuitableFor (DietType set), Allergens set

FoodItem.IngredientId? links free-text items to the catalogue (fuzzy match on name/aliases; user can override).
```

Enums:
- `DietType`: Omnivore, Flexitarian, Pescatarian, Vegetarian, Vegan
  - **Flexitarian** = mostly plant-based, meat/fish allowed occasionally. Meat and fish recipes are never excluded, but get a stronger ranking penalty than for Omnivore, and suggestions should show at least 2 vegetarian/vegan options for every meat option.
- `Allergen`: the 14 EU major allergens (Gluten, Crustaceans, Eggs, Fish, Peanuts, Soy, Milk, TreeNuts, Celery, Mustard, Sesame, Sulphites, Lupin, Molluscs)

### Core rules (live in `EatUp.Core`, fully unit-tested)
- **Effective expiry** = min(ExpiryDate, OpenedDate + OpenedShelfLifeDays) when opened.
- **Urgency**: Expired (<0 days), Today (0), Soon (1–2), Later (>2). Computed in the user's time zone.
- **Suggestion ranking** for a set of urgent items:
  1. Exclude recipes containing any user allergen.
  2. Exclude recipes not suitable for the user's diet.
  3. Score = (number of urgent items used × 10, more weight the closer to expiry) + (other active items used × 2) − (missing required ingredients × 3) + plant-forward bonus (vegan +3, vegetarian +2, pescatarian +1; meat penalty larger for Flexitarian).
  4. Return top N with which of the user's items each recipe uses.
- **Default expiry**: when the user picks a catalogue ingredient + storage location, prefill expiry from `DefaultShelfLifeDays` (user can change it).
- **Daily notification decision** (pure function: profile + items + top suggestion + `now` → `Notification?`):
  - Send only if notifications are enabled, local time in the user's zone is at or after `NotificationTime`, `LastNotifiedOn` is before today (local), and at least one active item is Today or Soon.
  - At most one notification per user per day. Text names the most urgent items and the top suggestion, e.g. *"Eggs expire tomorrow 🥚 — how about a veggie omelette?"*; payload includes `url: "/cook"`.
  - Handle DST changes and users in different time zones (test both).

## 6. API (MVP)

All under `/api`, cookie-auth except the auth endpoints, the VAPID public key and the auth config.

```
GET    /api/auth/config                 -> { googleClientId? }  (frontend hides the Google button when absent)
POST   /api/auth/register | /login?useCookies=true   (MapIdentityApi)
POST   /api/auth/google                 { idToken } -> validates with Google, finds/creates user, sets auth cookie
POST   /api/auth/logout
GET    /api/auth/me                     -> current user + profile, 401 if not signed in
GET    /api/profile                     PUT /api/profile
GET    /api/items?status=active         POST /api/items
GET    /api/items/{id}                  PUT /api/items/{id}    DELETE /api/items/{id}
POST   /api/items/{id}/consume          { amount?, outcome: Used|Discarded }
GET    /api/items/expiring?days=2       -> items + urgency
GET    /api/ingredients?search=egg      (autocomplete + default shelf life)
GET    /api/suggestions?days=2&limit=5  -> ranked recipes + which items they use
GET    /api/push/vapid-public-key
POST   /api/push/subscriptions          { endpoint, keys: { p256dh, auth } }  (upsert by endpoint)
DELETE /api/push/subscriptions          { endpoint }

Development environment only:
POST   /api/dev/push/test               sends a test notification to the current user's devices
POST   /api/dev/run-daily-check         runs the daily notification logic for the current user now, ignoring LastNotifiedOn
POST   /api/dev/reset-demo-data         restores the demo user's items (dates relative to today)
```

## 7. Phases

### Phase 0 — Scaffolding
- Create `backend/` solution with the three projects + test project; `dotnet build` and `dotnet test` pass.
- Create `web/` with Vite React-TS template, Tailwind, React Router, ESLint + Prettier, Vitest; `npm run lint`, `npm test`, `npm run build` (includes `tsc --noEmit`) pass.
- Root `package.json` with `dev` (backend + web via `concurrently`), `test`, `lint`, `gen:api` scripts. Configure Vite dev proxy `/api` → backend.
- Add `CLAUDE.md` with: how to run everything locally, coding conventions, the zero-cost rule from section 0, "regenerate API types after changing endpoints".
- Add `.gitignore` for .NET, Node, `backend/data/`, and local secrets.
- **Accept:** `npm run dev` starts both; the web app calls a `/api/health` endpoint through the proxy; a placeholder test passes in each project.

### Phase 1 — Backend inventory & auth
- EF Core `DbContext`, `FoodItem`, `UserProfile`, migrations (applied on startup in Development), Identity with cookie auth (HttpOnly, SameSite=Strict). API returns 401 instead of redirecting to a login page.
- `/api/auth/google` behind an `IGoogleTokenValidator` interface so tests can use a fake. The Google client ID is optional configuration; when absent, `/api/auth/config` omits it and the endpoint returns 404. Link Google logins to an existing account with the same verified email.
- Item CRUD + consume endpoints, scoped to the current user.
- Expiry/urgency logic in `EatUp.Core` with unit tests (include time-zone and opened-item cases).
- Development seed: a demo user (`demo@eatup.local` / `Demo123!`, as documented in `GETTING_STARTED.md`) with items whose expiry dates are relative to today (eggs tomorrow, avocado today, milk in 2 days, cheese in 10 days…).
- OpenAPI document exposed in Development.
- **Accept:** integration tests cover register → login (email and faked Google) → create item → list sorted by expiry → consume → logout; users cannot see each other's items; the app runs with no Google client ID configured.

### Phase 2 — Ingredient catalogue & recipes
- Seed ~80 common perishable ingredients (dairy, eggs, meat, fish, vegetables, fruit, bread, tofu…) with allergens, diet flags and shelf life.
- Seed ~60 simple recipes in `data/recipes.seed.json`, **majority vegetarian/vegan**, covering common leftovers (eggs → omelette, shakshuka, frittata; avocado → guacamole, avocado toast; milk → pancakes, porridge; cream cheese → pasta sauce…). Write these by hand; no paid recipe APIs.
- Suggestion engine in `EatUp.Core` + `/api/suggestions`, `/api/ingredients`.
- **Accept:** unit tests prove a vegan never gets an egg recipe, a nut-allergic user never gets a recipe with tree nuts, a flexitarian gets ≥2 plant-based suggestions per meat one, and urgent items rank first.

### Phase 3 — Web app: inventory, suggestions & profile
- Generate TS types from OpenAPI; `openapi-fetch` client with `credentials: "include"`; TanStack Query hooks. On 401, route to login.
- Mobile-first screens: Login/Register (email; "Sign in with Google" only when configured), Inventory (grouped by urgency, filter by location), Add/Edit item (ingredient autocomplete prefills category + expiry; native date input; storage location), Item actions (used / thrown away / partial), Cook now (suggestions for items expiring within 2 days, showing which of your items each recipe uses), Profile (diet, allergies, notification time; time zone auto-detected via `Intl.DateTimeFormat().resolvedOptions().timeZone`).
- **Accept:** in Chrome device mode a user can register, add "Eggs, fridge, expires in 2 days", see it flagged as Soon, see an egg recipe on Cook now, and mark the eggs used. Playwright smoke test covers this flow against the local dev servers.

### Phase 4 — PWA & Web Push
- Web app manifest (name, short_name, icons 192/512 + maskable, `display: "standalone"`, theme colour, `start_url: "/"`), Apple touch icon and meta tags.
- Service worker (`src/sw.ts`): precache the app shell; network-only for `/api` (the MVP needs the backend running; show a friendly message when it isn't reachable); `push` handler shows the notification; `notificationclick` focuses an open window or opens the payload `url`. Make sure the service worker also works in `npm run dev` (vite-plugin-pwa `devOptions`).
- Backend: VAPID keys from user secrets, `PushSubscription` entity + endpoints, `IPushSender` wrapping `WebPush`; delete subscriptions that return 404/410.
- `DailyNotificationJob : BackgroundService` runs every 15 minutes while the backend is running, applies the Core notification rule per user, sends to all their subscriptions, then sets `LastNotifiedOn`. Inject `TimeProvider` so tests control time.
- Dev endpoints and buttons on the Profile screen (Development only): **Send test notification**, **Run daily check now**, **Reset demo data**.
- Frontend push flow (`features/push`):
  - Detect support (`serviceWorker`, `PushManager`, `Notification`) and whether running installed (`display-mode: standalone`).
  - **iPhone/iPad in Safari, not installed:** show "Add to Home Screen" instructions (Share → Add to Home Screen) instead of a permission button.
  - Ask for permission **only from a user tap** on an explanation card ("Get a heads-up before food goes off"), never on page load.
  - Subscribe with the VAPID public key, post the subscription; re-sync it on every app start; unsubscribe on logout and when the user turns notifications off.
- **Accept:** unit tests for the notification rule (time zones, DST, once per day, nothing urgent → nothing sent) and the job with a fake `IPushSender`. Manually, on `http://localhost:5173` in desktop Chrome: install the PWA, enable notifications, log in as the demo user, press **Run daily check now**, receive *"Eggs expire tomorrow…"*, and tapping it opens Cook now.

**MVP done here.** Write a short `docs/mvp-review.md`: what works, known gaps (e.g. email confirmation, password reset, offline editing), and suggested next step.

### Phase 5 — Barcode scanning
- Camera via `getUserMedia` on the Add screen (works on `localhost`); use the native `BarcodeDetector` where available, otherwise `@zxing/browser` (needed on iOS Safari). Backend proxies **Open Food Facts** (free, no API key) to fill name, category, allergens. User still confirms the expiry date.

### Phase 6 — Camera label reading
- **Decide with the product owner before starting**, because the obvious AI option costs money:
  - Free, local default: **Tesseract.js** OCR in the browser + a date parser for common formats ("BEST BEFORE 12.11.26", "Use by 12 NOV"). Lower accuracy, zero cost.
  - Paid option: Claude vision via the backend (needs API credits and a card), behind an `ILabelReader` interface with a fake for tests.
- Either way: photo via `<input type="file" accept="image/*" capture="environment">`, resize client-side, show a prefilled form for confirmation, never auto-save.

### Phase 7 — Weekly meal planner & health feedback
- `MealPlan` (week) → `PlannedMeal` (day, slot: breakfast/lunch/dinner, recipe or free text).
- Nutrition tagging on recipes (protein source, has fish/omega-3, vegetables portions, fast-food/ultra-processed flag).
- **Rule-based** weekly analysis (no paid AI): e.g. "no fish/omega-3 source this week" (suggest plant alternatives like walnuts, flax, chia for vegans), "low protein on 3 days", "fast food 4×", "red meat 5×—try swapping two for legumes".
- Planner prefers recipes that use items about to expire.

### Phase 8 — Insights, households & offline
- Waste stats from Used vs Discarded history; shared household inventory with members.
- Offline: cache inventory reads in the service worker / IndexedDB and queue writes for sync.

## 8. Conventions for the agent

- Follow section 0: nothing paid, nothing that needs a card; ask before adding any external service.
- TDD for everything in `EatUp.Core` (expiry, ranking, notification rule). Keep domain logic out of endpoints and React components.
- Dates: store `ExpiryDate` as `DateOnly`; compute urgency and notification timing against the user's time zone, never server-local time. Use `TimeProvider` for "now".
- Every endpoint is user-scoped; add a test for cross-user access whenever you add one. Dev endpoints must not be mapped outside the Development environment.
- Regenerate the TS API types after any API change; never hand-write API types in the web app.
- Never commit secrets (VAPID private key, Google client ID); use .NET user secrets.
- Small commits per completed step, message describes the user-visible change.
- No new infrastructure (Docker, Postgres, queues, hosting) unless a phase explicitly needs it and it satisfies section 0.

## 9. Decisions made with the product owner

- Platform: installable PWA (not a native app); notifications via Web Push.
- No login in the MVP (single local user). Later: email + password, Google sign-in optional.
- UI language: English only.
- Flexitarian is a diet option.
- Local-first, zero cost: no paid hosting or services; only free, no-card services where something external is unavoidable. No hosting plan for now.
