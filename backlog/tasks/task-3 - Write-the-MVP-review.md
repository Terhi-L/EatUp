---
id: TASK-3
title: Write the MVP review
status: To Do
assignee: []
created_date: '2026-11-05T13:00:00Z'
updated_date: '2026-11-05T14:44:00Z'
labels:
  - docs
  - ready-for-agent
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
- [ ] #1 docs/mvp-review.md lists what works today, per MVP feature in PLAN.md § 2
- [ ] #2 It lists the known gaps and deviations (no login, EnsureCreated instead of migrations, hand-written API types, no ESLint/Playwright, machine-drafted nb/fi texts, partial discards not recorded)
- [ ] #3 It names the suggested next step and links to the Backlog instead of duplicating the task list
- [ ] #4 PLAN.md status line points to the review
<!-- AC:END -->
