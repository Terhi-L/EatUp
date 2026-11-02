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
