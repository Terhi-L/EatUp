---
id: TASK-5
title: Add ESLint and a Playwright smoke test
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-08T15:10:00Z'
labels:
  - frontend
  - testing
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

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. ESLint flat config in web/ (typescript-eslint, react-hooks, react-refresh); npm run lint at root and in web; fix findings.
2. Playwright in web/e2e: config starts its own API (port 5081, temp DataDirectory, daily job off) and Vite (port 5174, proxy target from EATUP_API_URL), so the test never touches the user's database or running dev servers.
3. Smoke test: clear demo items via API, add Eggs in the fridge expiring in 2 days through the form, see it under Soon, see an egg recipe on Cook now, mark the eggs used.
4. Root npm run e2e; document in GETTING_STARTED.md; update docs/decisions.md.
<!-- SECTION:PLAN:END -->
