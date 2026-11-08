---
id: TASK-5
title: Add ESLint and a Playwright smoke test
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-08T15:06:00Z'
labels:
  - frontend
  - testing
  - ready-for-agent
milestone: m-0
dependencies: []
priority: high
type: chore
ordinal: 5000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
PLAN.md phases 0 and 3 call for ESLint and a Playwright smoke test; both were skipped in the short MVP (docs/decisions.md). Bigger features are coming, and there is no end-to-end check that the main flow still works. Must stay free and local.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 `npm run lint` lints the web app and passes
- [ ] #2 A Playwright test against the local dev servers adds "Eggs, fridge, expires in 2 days", sees it flagged as soon, sees an egg recipe on Cook now and marks the eggs used
- [ ] #3 A root script runs the e2e test, and GETTING_STARTED.md says how
- [ ] #4 docs/decisions.md entry about skipping ESLint/Playwright is updated
<!-- AC:END -->
