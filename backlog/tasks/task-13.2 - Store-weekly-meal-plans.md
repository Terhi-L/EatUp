---
id: TASK-13.2
title: Store weekly meal plans
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-24T10:02:00Z'
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

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Domain/MealPlanning (tests first, no EF/HTTP): WeekStart(date) gives the Monday of its week; Validate checks a planned meal has exactly one of recipe key or free text, the recipe exists, free text is non-blank and at most 200 chars, the slot is defined and the date falls inside the week.
2. Entities MealPlan (WeekStart, unique) and PlannedMeal (Date, Slot, RecipeKey or FreeText); EF config and migration AddMealPlans.
3. Endpoints under /api/meal-plans: GET current (this week in the user's time zone), GET {week} (any date in the week), POST {week}/meals, PUT and DELETE {week}/meals/{id}. All declare their 400/404 responses.
4. Endpoint tests for CRUD, validation and the user-time-zone week; npm run gen:api; decisions entry for the design choices.
<!-- SECTION:PLAN:END -->
