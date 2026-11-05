---
id: TASK-4
title: Switch from EnsureCreated to EF Core migrations
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-05T15:32:00Z'
labels:
  - backend
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
- [ ] #4 A database created by EnsureCreated (no migrations history) is reset once on startup with a clear console message, keeping `backend/data/vapid.json`; from the first migration on, startup only upgrades and never deletes data. Logged in docs/decisions.md
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add Microsoft.EntityFrameworkCore.Design; generate InitialCreate migration with dotnet-ef into Data/Migrations.
2. Extract startup DB preparation into a DatabaseSetup class: if the DB has tables but no __EFMigrationsHistory (EnsureCreated era), delete the DB file once with a console warning (vapid.json is a separate file, untouched); then MigrateAsync; then seed.
3. Tests: legacy EnsureCreated DB gets reset and migrated; migrated DB with data keeps data across restarts; API tests run on the migrated schema.
4. Update AGENTS.md, GETTING_STARTED.md, docs/decisions.md; run checks.
<!-- SECTION:PLAN:END -->
