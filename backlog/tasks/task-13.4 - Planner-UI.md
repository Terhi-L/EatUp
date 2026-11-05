---
id: TASK-13.4
title: Planner UI
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
labels:
  - frontend
milestone: m-3
dependencies:
  - TASK-13.2
  - TASK-13.3
parent_task_id: TASK-13
priority: medium
type: feature
ordinal: 13400
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The screen where users fill in a week and see the feedback. Mobile-first, following the existing visual design (web/src/index.css tokens, icons in web/src/icons.tsx).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 A user can pick a recipe or type free text for any day and slot, and move between weeks
- [ ] #2 Weekly feedback is shown on the same screen and updates after each change
- [ ] #3 Texts exist in en, nb and fi; component tests cover adding and removing a meal
<!-- AC:END -->
