---
id: TASK-4
title: Switch from EnsureCreated to EF Core migrations
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-05T14:50:00Z'
labels:
  - backend
  - ready-for-agent
milestone: m-0
dependencies: []
priority: high
type: chore
ordinal: 4000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The database is built with `EnsureCreated`, so every EF model change means deleting `backend/data/` and losing all items (docs/decisions.md 2026-11-02; Qodo flagged it on PR #4). Almost every planned feature (waste history, meal plans, accounts) changes the model, so migrations must come first. VAPID keys also live in `backend/data/vapid.json` and must survive.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 An initial migration matches the current model, and the API applies pending migrations on startup
- [ ] #2 AGENTS.md and GETTING_STARTED.md describe how to add a migration instead of deleting backend/data/
- [ ] #3 Tests run against the migrated schema; npm test and npm run typecheck pass
<!-- AC:END -->
