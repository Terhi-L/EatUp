---
id: TASK-15
title: 'Work offline: cached inventory and queued edits'
status: To Do
assignee: []
created_date: '2026-11-05T13:02:00Z'
labels:
  - frontend
  - pwa
  - needs-decision
milestone: m-4
dependencies: []
priority: low
type: feature
ordinal: 15000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The PWA needs the backend running; offline it only shows a friendly error (PLAN.md phase 8). In a shop or a basement kitchen the user cannot check or update the fridge. Conflict handling must be agreed with the product owner before starting.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Inventory reads work offline from a service-worker/IndexedDB cache
- [ ] #2 Adds, edits and consumes made offline are queued and synced when back online, with the X-EatUp header
- [ ] #3 The UI shows when it is offline and when queued changes are pending
- [ ] #4 Tests cover queueing and replay order
<!-- AC:END -->
