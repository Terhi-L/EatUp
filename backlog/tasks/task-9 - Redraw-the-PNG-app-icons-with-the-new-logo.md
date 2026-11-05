---
id: TASK-9
title: Redraw the PNG app icons with the new logo
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
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
