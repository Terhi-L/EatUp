---
id: TASK-13.2
title: Store weekly meal plans
status: Done
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-24T11:15:00Z'
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

PR: https://github.com/Terhi-L/EatUp/pull/23. Quality gate (AGENTS.md rule 7, PR #22) applied: round 1 on 1a2f348 found 2 Medium (Rule violation: JSON attributes in the domain record; Bug: contract required unused fields), both fixed in e0e73ef; round 2 complete review on e0e73ef has 0 open findings and no Low findings.
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Weekly meal plans are stored and served by the API (PLAN.md phase 7, storage only; UI is TASK-13.4). Domain/MealPlanning (tests first) defines Monday-based weeks and the recipe-or-free-text rule; MealPlan (unique week) and PlannedMeal are added by migration AddMealPlans with a check constraint; /api/meal-plans serves the current week in the user's time zone, any week by date, and CRUD for meals, with PlannedMealInput as the API request shape. Verification: 235 xUnit + 37 Vitest pass, typecheck and oxlint clean; Qodo local review (3 findings fixed before commit) and PR review (2 Medium fixed) leave no open findings. Follow-ups: translated error texts for the new fields come with TASK-13.4.
<!-- SECTION:FINAL_SUMMARY:END -->
