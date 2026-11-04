---
id: TASK-1
title: Norwegian and Finnish language support
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-04T12:30:00Z'
updated_date: '2026-11-04T13:22:00Z'
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
- [ ] #1 User can pick English, Norsk or Suomi on the Profile page; the choice persists in the profile and the UI switches immediately
- [ ] #2 First visit with no saved language uses the browser language when it is nb/no or fi, else English
- [ ] #3 Catalog search, ingredient names, recipe suggestions and push notifications use the profile language
- [ ] #4 Norwegian/Finnish aliases match products (e.g. lettmelk → milk), and barcode lookups prefer nb/no/fi product names
- [ ] #5 Units show in the active language (pcs ↔ stk/kpl), and a scanned package size in kpl parses as pcs
- [ ] #6 API errors appear in the active language
- [ ] #7 Translation and language-switch tests exist and `npm test` + `npm run typecheck` pass
- [ ] #8 i18n decisions are logged in `docs/decisions.md`
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
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Done so far (uncommitted): backend `Language` enum + nullable `UserProfile.Language` in ProfileDto/ProfileUpdate; `Seed/catalog.Norwegian.json` and `catalog.Finnish.json`; `Catalog.NameOf/TextOf/Search(query, language)`; Nordic alias suffix matching; `NotificationText`; localised suggestions and Open Food Facts names; TranslationTests + API language test (94 backend tests passing). Frontend: `web/src/i18n/{en,nb,fi}.ts`, live-binding `strings.ts` switcher (`setLanguage`, `useLanguage`, `detectLanguage`, `languageNames`, `unitLabel`, `localeOf`), localised api errors, `lib/profileSync.ts`, `lib/i18n.test.ts`; App.tsx `tabs` is now a function plus `syncProfile`.
<!-- SECTION:NOTES:END -->
