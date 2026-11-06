---
id: TASK-4
title: Switch from EnsureCreated to EF Core migrations
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-06T12:41:00Z'
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
- [x] #1 An initial migration matches the current model, and the API applies pending migrations on startup
- [x] #2 AGENTS.md and GETTING_STARTED.md describe how to add a migration instead of deleting backend/data/
- [x] #3 Tests run against the migrated schema; npm test and npm run typecheck pass
- [x] #4 A database created by EnsureCreated (no migrations history) is reset once on startup with a clear console message, keeping `backend/data/vapid.json`; from the first migration on, startup only upgrades and never deletes data. Logged in docs/decisions.md
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add Microsoft.EntityFrameworkCore.Design; generate InitialCreate migration with dotnet-ef into Data/Migrations.
2. Extract startup DB preparation into a DatabaseSetup class: if the DB has tables but no __EFMigrationsHistory (EnsureCreated era), delete the DB file once with a console warning (vapid.json is a separate file, untouched); then MigrateAsync; then seed.
3. Tests: legacy EnsureCreated DB gets reset and migrated; migrated DB with data keeps data across restarts; API tests run on the migrated schema.
4. Update AGENTS.md, GETTING_STARTED.md, docs/decisions.md; run checks.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
InitialCreate migration in Data/Migrations (dotnet-ef 10.0.10, Microsoft.EntityFrameworkCore.Design 10.0.14 as a private build-time package). DatabaseSetup.PrepareAsync: legacy = DB exists, has tables, no applied migrations -> EnsureDeleted with a warning, then MigrateAsync.
Evidence: DatabaseSetupTests.Migrations_match_the_model (HasPendingModelChanges false), A_new_database_is_created_from_migrations, A_pre_migration_database_is_reset_once_and_keeps_the_vapid_keys, A_migrated_database_keeps_its_data_on_restart; ApiTests start through Program, so they run on the migrated schema. npm run typecheck passes; npm test: backend 109, web 30.
Note for the user: the existing local backend/data/eatup.db (EnsureCreated era) will be reset on the next npm run dev, as agreed.

Qodo PR #9 fix: the reset only deletes a database whose tables exactly match the model; otherwise startup throws (DatabaseSetupTests.An_unrelated_database_is_never_deleted). npm test: backend 110, web 30.
<!-- SECTION:NOTES:END -->
