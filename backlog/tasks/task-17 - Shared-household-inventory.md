---
id: TASK-17
title: Shared household inventory
status: To Do
assignee: []
created_date: '2026-11-05T13:02:00Z'
labels:
  - backend
  - frontend
milestone: m-4
dependencies:
  - TASK-16
priority: low
type: feature
ordinal: 17000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
People living together share one fridge, so each tracking their own copy causes duplicates and missed reminders (PLAN.md phase 8).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 A user can create a household and invite members; members see and edit the same inventory
- [ ] #2 Each member gets their own daily reminder for the shared items
- [ ] #3 Leaving a household keeps the household data for the others; tests cover access rules
<!-- AC:END -->
