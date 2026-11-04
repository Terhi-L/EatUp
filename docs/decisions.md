# Decisions

Short log of decisions and deliberate deviations from [PLAN.md](../PLAN.md). Newest first.

## 2026-11-02 · Visual design applied

The UI now follows the "EatUp" design canvas (ink outlines, hard shadows, pastel tones on grid paper, Caveat Brush + Shantell Sans, SVG icons instead of emoji). Tokens live in `web/src/index.css` (`@theme`), icons in `web/src/icons.tsx`. Not carried over from the design yet:

- **No Shopping tab**: there's no shopping-list feature, so the nav has 3 tabs.
- **No search** (phone header button, desktop search box) and **no "Tonight, maybe?" recipe card** on the desktop inventory.
- **Desktop keeps four urgency groups** instead of merging "Gone off" and "Eat today" into one "Eat now" column.
- **PNG app icons** (`icon-192/512`, `apple-touch-icon`) still show the old green logo; only `favicon.svg` was redrawn.

## 2026-11-02 · Short MVP built

The product owner asked for a "simple short version" of the MVP **without login**. Compared with the plan:

- **No accounts or login.** A single implicit local user: one `UserProfile` row (Id = 1) and no `UserId` on items. Adding accounts later means adding Identity plus a `UserId` column and scoping every query.
- **One backend project** (`EatUp.Api`) instead of Api/Core/Infrastructure. Pure domain rules sit in `Domain/` with no EF or HTTP dependencies, so they can move to their own project later.
- **`EnsureCreated` instead of EF migrations.** After changing the model, delete `backend/data/`. Switch to migrations before there's data worth keeping.
- **Catalogue is a JSON file loaded into memory** (`Seed/catalog.json`), not database tables. Recipes and ingredients are read-only for now.
- **Hand-written TypeScript API types** (`web/src/api.ts`) instead of types generated from OpenAPI. Keep them in sync with `Endpoints.cs` by hand until the API grows.
- **No ESLint or Playwright yet.** Type-checking (`tsc -b`) and Vitest cover the web app.
- **VAPID keys are generated into `backend/data/vapid.json`** instead of user secrets, so `npm run dev` works with zero setup. They can be overridden with `Push:PublicKey` / `Push:PrivateKey` configuration.
- **Suggestions never use expired items**, since use-by dates may be unsafe. Expired items still show in the inventory.
- **Partial use** reduces the quantity and keeps the item active. A partial "thrown away" amount isn't recorded as waste yet.
- **"Meat-free"** in the UI means no meat or fish (dairy and eggs allowed). Flexitarian suggestions show at most one meat/fish option per two meat-free ones.

## 2026-11-03 · Fixes from the PR #1 code review

- **Writes need an `X-EatUp` header.** With no login, this stops other websites from triggering POST/PUT/DELETE on the local API (a custom header forces a CORS preflight, which the API never allows). `web/src/api.ts` always sends it.
- **Profile updates are `PATCH /api/profile` with only the changed fields**, so overlapping edits and the time-zone sync can't overwrite each other.
- **The daily reminder only counts as sent once a device received it.** Failed pushes keep their subscription; 401/403/404/410 remove it.
- **Flexitarians still get one meat/fish idea** when nothing plant-based fits.

## 2026-11-03 · Barcode scanning (Phase 5)

- **Photo, not live video.** "Scan barcode" opens the camera through `<input type="file" capture="environment">` and decodes the photo with `@zxing/browser`, instead of a `getUserMedia` stream with the native `BarcodeDetector`. One code path works on iPhone and Android, and over plain `http://` on the local network, where browsers block live camera access. The decoder is loaded on the first scan, so the app still starts fast.
- **Category from Open Food Facts tags, exact matches only.** The most specific tag that exactly names a catalogue ingredient wins (`en:milks` → milk). Loose word matching is used only on the product name, as when typing, because it would turn "plant milks" into milk.
- **The name becomes the catalogue name** (e.g. "Milk") when recognised, so the form suggests a shelf life; otherwise the package name is kept. A barcode has no expiry date, so the user still confirms it, and fields left empty are highlighted.

## 2026-11-03 · European storage wording

- **`Cupboard` instead of `Pantry`** as a storage location, since walk-in pantries are rare in Nordic/European kitchens. The icon is a cabinet, not a bottle.
- **Opened food can move to the fridge.** Catalogue entries may set `openedLocation` (tins of chickpeas, coconut milk, tomatoes). Ticking "It's opened" in the form switches the location and says so; the user can switch it back.

## 2026-11-04 · Norwegian and Finnish

- **Typed dictionaries, no i18n package.** `web/src/i18n/en.ts` is the source and defines the `Strings` type, so `nb.ts` and `fi.ts` can't miss a key. `strings.ts` swaps the active dictionary, and the app re-renders from the root.
- **The language is stored in the profile** (`UserProfile.Language`) as well as `localStorage`, because the server words push notifications, suggestions, ingredient names and Open Food Facts product names in it. Before a language is chosen, the browser language decides, falling back to English.
- **Catalogue translations are side files** (`Seed/catalog.Norwegian.json`, `catalog.Finnish.json`) keyed by the English catalogue keys, so recipes stay one list. Nordic aliases of 4+ letters also match word endings (lettmelk → milk), since compound words are common.
- **API error text is chosen by the client.** The backend stays English; `api.ts` shows validation errors by field and other errors by HTTP status (`errors.byStatus`), never the backend's title.
- **Plant-based names never match dairy.** A name with a plant word (soy, oat, havre, kaura, mandel, plante…) skips milk and butter, so word-ending matches can't turn "soyamelk" into milk; such items get no ingredient and need a date.
- **Search matches the start of any word** in any language ("milk" → Coconut milk) and returns every alias, so the form recognises a name typed in another language than the profile's.
- **Units are stored in English** (`pcs`, `can`) and only shown translated (stk/kpl, boks/purkkia). Typed or scanned stk/kpl are saved as `pcs`, so switching language doesn't strand old units.
- **The Norwegian and Finnish texts are machine-drafted** and need review by a native speaker.
