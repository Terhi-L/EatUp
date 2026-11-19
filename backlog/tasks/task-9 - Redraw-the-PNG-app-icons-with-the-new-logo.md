---
id: TASK-9
title: Redraw the PNG app icons with the new logo
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-19T14:21:00Z'
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
- [ ] #1 All PNG icons match favicon.svg, with a maskable safe zone
- [ ] #2 The installed PWA and iPhone home screen show the new logo
- [ ] #3 The decisions entry is updated
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add scripts/gen-icons.mjs (npm run gen:icons): render web/public/favicon.svg with the Playwright Chromium already used for e2e, so the PNGs are regenerated from the one source of truth instead of hand-drawn.
2. Variants: icon-192/icon-512 (purpose any) = favicon as-is; icon-maskable-512 = full-bleed background with the artwork shrunk into the 80% safe zone; apple-touch-icon 180 = full-bleed background (iOS turns transparency black and rounds corners itself).
3. Point the manifest's maskable entry at the new icon-maskable-512.png.
4. Verify: pixel check that maskable artwork stays inside the safe-zone circle, build and inspect the generated manifest, typecheck + tests.
5. Update the docs/decisions.md entry (PNG icons no longer a gap).
<!-- SECTION:PLAN:END -->
