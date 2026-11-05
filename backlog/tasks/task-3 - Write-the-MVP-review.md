---
id: TASK-3
title: Write the MVP review
status: Done
assignee:
  - '@claude'
created_date: '2026-11-05T13:00:00Z'
updated_date: '2026-11-05T15:31:00Z'
labels:
  - docs
milestone: m-0
dependencies: []
priority: high
type: docs
ordinal: 3000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
PLAN.md says the MVP ends with a short `docs/mvp-review.md`, and it was never written. Since then the build grew past the MVP (barcode scanning, Norwegian/Finnish), so a fresh agent or the product owner has no single page saying what works, what is known to be missing and what comes next. Inputs: PLAN.md, docs/decisions.md, the app itself.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 docs/mvp-review.md lists what works today, per MVP feature in PLAN.md § 2
- [x] #2 It lists the known gaps and deviations (no login, EnsureCreated instead of migrations, hand-written API types, no ESLint/Playwright, machine-drafted nb/fi texts, partial discards not recorded)
- [x] #3 It names the suggested next step and links to the Backlog instead of duplicating the task list
- [x] #4 PLAN.md status line points to the review
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Verify each MVP feature in PLAN.md § 2 against the code (endpoints, web features, service worker, push, daily job, dev endpoints, tests).
2. Write docs/mvp-review.md: what works, beyond-MVP additions, known gaps/deviations, suggested next step linking to Backlog.
3. Point the PLAN.md status line to the review.
4. Run npm run typecheck and npm test.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Each MVP row in PLAN.md § 2 checked against code: Endpoints.cs routes, web/src/App.tsx routes, sw.ts push/notificationclick, vite.config.ts manifest, push.ts iOS install guidance, DailyCheck.cs periodic timer, dev endpoints.
Found and listed as a gap: /api/shopping endpoints exist with no UI (decisions.md says there is no shopping feature).
npm run typecheck passes; npm test: backend 105, web 30 passed (docs-only change).

Ready for review: https://github.com/Terhi-L/EatUp/pull/8 (Qodo: 2 low findings, both rejected with reasons in Agent round 1).
<!-- SECTION:NOTES:END -->
