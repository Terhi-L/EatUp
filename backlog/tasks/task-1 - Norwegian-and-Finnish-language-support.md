---
id: TASK-1
title: Norwegian and Finnish language support
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-04T12:30:00Z'
updated_date: '2026-11-04T20:52:00Z'
labels:
  - i18n
  - frontend
  - backend
dependencies: []
priority: medium
type: feature
ordinal: 1000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
EatUp targets Nordic users, but the UI, catalog, recipes, notifications and barcode product names were English-only, so Norwegian and Finnish users and products were poorly served. Approved 2026-11-03: add Norwegian (Bokmål) and Finnish with an in-app language switch. Chosen approach: typed dictionaries (`web/src/i18n/en.ts` as source with a `Strings` type, plus `nb.ts`/`fi.ts`), no i18n package. The drafted nb/fi texts need review by native speakers.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 User can pick English, Norsk or Suomi on the Profile page; the choice persists in the profile and the UI switches immediately
- [x] #2 First visit with no saved language uses the browser language when it is nb/no or fi, else English
- [x] #3 Catalog search, ingredient names, recipe suggestions and push notifications use the profile language
- [x] #4 Norwegian/Finnish aliases match products (e.g. lettmelk → milk), and barcode lookups prefer nb/no/fi product names
- [x] #5 Units show in the active language (pcs ↔ stk/kpl), and a scanned package size in kpl parses as pcs
- [x] #6 API errors appear in the active language
- [x] #7 Translation and language-switch tests exist and `npm test` + `npm run typecheck` pass
- [x] #8 i18n decisions are logged in `docs/decisions.md`
<!-- AC:END -->

## Definition of Done
<!-- DOD:BEGIN -->
- [ ] #1 nb/fi texts sent for native-speaker review
<!-- DOD:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. App.tsx: change both `tabs.map(` to `tabs().map(`.
2. ProfilePage: language chips from `languageNames` with `lang` from `localeOf`; click runs `setLanguage(l)` + `update({ language: l })`, invalidate all queries on settle.
3. Units: ItemFormPage default unit = `strings.form.defaultUnit`, scanned `pcs` via `unitLabel`; InventoryPage shows `unitLabel(item.unit)`; backend `PackageSize` maps `kpl` → `pcs`.
4. Run typecheck and tests (backend: `dotnet test backend --artifacts-path "$TEMP/claude/eatup-artifacts"` while dev server locks bin/).
5. Log decisions in docs/decisions.md.
6. Delete backend/data/ (new Profile.Language column) and restart.

7. AC #6 gap: translate non-validation API errors on the client by HTTP status (403/404/502), never show the backend's English title; test in web/src/api.test.ts.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Done so far (uncommitted): backend `Language` enum + nullable `UserProfile.Language` in ProfileDto/ProfileUpdate; `Seed/catalog.Norwegian.json` and `catalog.Finnish.json`; `Catalog.NameOf/TextOf/Search(query, language)`; Nordic alias suffix matching; `NotificationText`; localised suggestions and Open Food Facts names; TranslationTests + API language test (94 backend tests passing). Frontend: `web/src/i18n/{en,nb,fi}.ts`, live-binding `strings.ts` switcher (`setLanguage`, `useLanguage`, `detectLanguage`, `languageNames`, `unitLabel`, `localeOf`), localised api errors, `lib/profileSync.ts`, `lib/i18n.test.ts`; App.tsx `tabs` is now a function plus `syncProfile`.

Finished: language picker on ProfilePage (setLanguage + PATCH, invalidates all queries); units shown via unitLabel and saved via new storedUnit; PackageSize maps kpl → pcs; tests added (web units test, backend "10 kpl"). typecheck + web 24 + backend 95 tests pass. Decisions logged 2026-11-04.

Still to verify in the running app (after deleting backend/data/): language picker switches UI and persists (#1), catalog/suggestions/notifications in profile language (#3), barcode lookup names (#4), units shown as stk/kpl (#5), API errors localised (#6). Automated tests cover parts of #3, #4 and #6 only.

2026-11-04: AC #6 gap fixed - api.ts now picks every error message client-side (field, then errors.byStatus 403/404/502, then requestFailed); new web/src/api.test.ts. typecheck + web 27 tests pass. Verified against the running API: profile language persists; Finnish 'maito' and Norwegian 'lettmelk' find milk (Maito/Melk); suggestions in Finnish; daily check notification in Norwegian; barcode 7038010009457 (nb) and 6408430000135 (fi) return local product names. Not verified: #1 picker UI and #5 unit display (Chrome extension not connected).

2026-11-04: User verified #1 (picker switches UI and persists) and #5 (units shown as stk/kpl) in the running app. Follow-up from that check: demo units bag/tub/loaf/bulb changed to pcs; unitLabel/storedUnit now use a per-language units map (pcs, can → nb 'boks' (unconfirmed), fi 'purkkia'). Item names stay as entered (demo items seed in English) by design. typecheck + web 27 + backend 95 tests pass.
<!-- SECTION:NOTES:END -->
