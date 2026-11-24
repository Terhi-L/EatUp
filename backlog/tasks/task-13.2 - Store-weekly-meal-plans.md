---
id: TASK-13.2
title: Store weekly meal plans
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-24T11:07:00Z'
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
- [x] #1 Endpoints create, read, update and delete planned meals for a given week
- [x] #2 A planned meal is either a recipe or free text, never both or neither (400 otherwise)
- [x] #3 Weeks start on Monday in the user time zone
- [x] #4 Model is added as an EF migration; endpoint tests cover validation
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Domain/MealPlanning (tests first, no EF/HTTP): WeekStart(date) gives the Monday of its week; Validate checks a planned meal has exactly one of recipe key or free text, the recipe exists, free text is non-blank and at most 200 chars, the slot is defined and the date falls inside the week.
2. Entities MealPlan (WeekStart, unique) and PlannedMeal (Date, Slot, RecipeKey or FreeText); EF config and migration AddMealPlans.
3. Endpoints under /api/meal-plans: GET current (this week in the user's time zone), GET {week} (any date in the week), POST {week}/meals, PUT and DELETE {week}/meals/{id}. All declare their 400/404 responses.
4. Endpoint tests for CRUD, validation and the user-time-zone week; npm run gen:api; decisions entry for the design choices.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Evidence per AC (npm test: 234 xUnit + 37 Vitest pass; typecheck + oxlint clean):
#1 CRUD for a week: MealPlanApiTests.Plan_a_week_then_change_and_remove_meals, A_new_meal_can_be_read_from_its_location, A_meal_is_only_found_in_its_own_week.
#2 Recipe xor free text, 400 otherwise: MealPlanApiTests.An_invalid_meal_is_rejected (both, neither, blank, unknown recipe), MealPlanningTests; DB check constraint CK_PlannedMeals_RecipeOrFreeText.
#3 Monday weeks in the user's time zone: MealPlanningTests.A_week_starts_on_the_Monday_on_or_before_the_date, MealPlanApiTests.The_current_week_starts_on_Monday_in_the_users_time_zone (Sunday 23:30 UTC in Europe/Oslo gives the week of Monday 2026-11-30).
#4 Migration AddMealPlans (DatabaseSetupTests.Migrations_match_the_model passes); validation covered by the API tests above plus A_meal_without_a_slot_or_date_is_rejected_instead_of_defaulting.
Qodo local review before commit: round 1 found 3 (missing slot became breakfast, Location had no GET, concurrent first saves gave 500), all fixed test-first; round 2 resolved all 3 with none open.
<!-- SECTION:NOTES:END -->
