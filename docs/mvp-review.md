# MVP review

Status on 2026-11-05. Compares the build with the MVP in [PLAN.md](../PLAN.md) § 2; the reasons behind each deviation are in [decisions.md](decisions.md).

## What works

| MVP feature (PLAN.md § 2) | State today |
|---|---|
| Account sign-up / login | Moved out of the MVP: one implicit local user. |
| Add / edit / delete items | Works: name, category, quantity + unit, storage location (Fridge, Freezer, Cupboard, Counter), expiry date, optional opened date. Picking a catalogue ingredient suggests an expiry date. |
| Inventory sorted by expiry with urgency colours | Works: grouped as gone off / today / soon (1–2 days) / later. |
| Used, thrown away, partially used | Works. Partial use lowers the quantity and keeps the item active. |
| Diet and allergy profile | Works: five diets and the 14 EU allergens, plus notification time and time zone (detected from the browser). |
| Recipe suggestions for items expiring within 2 days | Works on Cook now: filtered by diet and allergies, ranked plant-forward, showing which of your items each recipe uses. Expired items are never suggested. |
| Installable PWA | Works: manifest, icons, custom service worker, "Add to Home Screen" guidance on iPhone. |
| Daily Web Push with a suggested dish | Works: a background check sends at most one reminder per day after the chosen time; tapping it opens Cook now. Dev-only buttons on Profile send a test push, run the daily check and reset demo data. |
| Runs locally with one command | Works: `npm run dev`; VAPID keys are generated on first run. |

Built beyond the MVP: barcode scanning with Open Food Facts lookup (phase 5), Norwegian and Finnish (UI, catalogue, suggestions, notifications), and the hand-drawn visual design.

## Known gaps and deviations

- **No login.** Single local user; accounts need Identity plus a user id on every row.
- **`EnsureCreated` instead of EF migrations.** Any model change means deleting `backend/data/` and losing all items.
- **Hand-written API types** in `web/src/api.ts`, kept in sync with `Endpoints.cs` by hand.
- **No ESLint or Playwright.** Type-checking and Vitest cover the web app; there is no end-to-end test of the main flow.
- **Norwegian and Finnish texts are machine-drafted** and still need a native-speaker review.
- **Partial discards are not recorded as waste**, and full consumes only change the item status, so there is no usable waste history yet.
- **One backend project** instead of Api/Core/Infrastructure; domain rules sit in `Domain/` so they can move later.
- **No offline editing**: the app needs the backend running and says so when it can't reach it.
- **Design gaps**: no inventory search, no "Tonight, maybe?" card, four urgency columns on desktop, and the PNG app icons still show the old logo.
- **A shopping-list API** (`/api/shopping`) exists in the backend with no screen using it.

## Suggested next step

Switch to EF Core migrations first, so data survives the model changes every later feature needs; then add ESLint and a Playwright smoke test, and generate the API types. The ordered work list lives in the Backlog (`backlog board`, or `backlog task list --status "To Do" --ready --sort ordinal --plain`).
