---
id: TASK-13.2
title: Store weekly meal plans
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-05T13:02:00Z'
labels:
  - backend
milestone: m-3
dependencies:
  - TASK-4
  - TASK-6
parent_task_id: TASK-13
priority: medium
type: feature
ordinal: 13200
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Users need to save a week of planned meals: `MealPlan` (week) with `PlannedMeal` (day, slot breakfast/lunch/dinner, recipe or free text), per PLAN.md phase 7. Single local user for now.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Endpoints create, read, update and delete planned meals for a given week
- [ ] #2 A planned meal is either a recipe or free text, never both or neither (400 otherwise)
- [ ] #3 Weeks start on Monday in the user time zone
- [ ] #4 Model is added as an EF migration; endpoint tests cover validation
<!-- AC:END -->
