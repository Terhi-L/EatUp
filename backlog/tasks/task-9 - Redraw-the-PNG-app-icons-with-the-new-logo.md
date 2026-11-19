---
id: TASK-9
title: Redraw the PNG app icons with the new logo
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-19T14:24:00Z'
labels:
  - frontend
  - design
milestone: m-1
dependencies: []
priority: low
type: chore
ordinal: 9000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Only `favicon.svg` was redrawn for the new visual design; `icon-192`, `icon-512` (incl. maskable) and `apple-touch-icon` still show the old green logo, so the installed PWA looks different from the app (docs/decisions.md 2026-11-02).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 All PNG icons match favicon.svg, with a maskable safe zone
- [x] #2 The installed PWA and iPhone home screen show the new logo
- [x] #3 The decisions entry is updated
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add scripts/gen-icons.mjs (npm run gen:icons): render web/public/favicon.svg with the Playwright Chromium already used for e2e, so the PNGs are regenerated from the one source of truth instead of hand-drawn.
2. Variants: icon-192/icon-512 (purpose any) = favicon as-is; icon-maskable-512 = full-bleed background with the artwork shrunk into the 80% safe zone; apple-touch-icon 180 = full-bleed background (iOS turns transparency black and rounds corners itself).
3. Point the manifest's maskable entry at the new icon-maskable-512.png.
4. Verify: pixel check that maskable artwork stays inside the safe-zone circle, build and inspect the generated manifest, typecheck + tests.
5. Update the docs/decisions.md entry (PNG icons no longer a gap).
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
- New `npm run gen:icons` (scripts/gen-icons.mjs) renders every PNG from web/public/favicon.svg with the Playwright Chromium already installed for e2e, so the PNGs can't drift from the favicon again; no new dependency.
- icon-192/icon-512 (purpose any) are the favicon as-is; icon-maskable-512.png (new, manifest maskable entry now points to it) and apple-touch-icon.png are full-bleed (tile colour read from favicon.svg, since iOS turns transparency black) with artwork shrunk to 80% for the maskable one.
- Evidence: pixel check found the maskable artwork reaches 29.9% of the width from centre (safe zone 40%) and 0 transparent pixels in both full-bleed icons; `vite build` + `vite preview`: manifest.webmanifest lists icon-192, icon-512, icon-maskable-512 (maskable), index.html links /apple-touch-icon.png, all four PNGs serve 200 image/png and are in the SW precache.
- `npm run typecheck` and `npm test` (110 xUnit, 31 Vitest) pass.
- Not checked on a real phone: already-installed apps keep the old icon until reinstalled (iOS always; Android refreshes on its own schedule).
<!-- SECTION:NOTES:END -->
