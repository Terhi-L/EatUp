---
id: TASK-18
title: Declare the 400 validation response on older endpoints
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-24T09:44:00Z'
updated_date: '2026-11-25T09:55:00Z'
labels:
  - backend
dependencies: []
references:
  - https://github.com/Terhi-L/EatUp/pull/20
priority: low
type: chore
ordinal: 18000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Qodo flagged on PR #20 that the consume endpoint returned a validation problem (400) without declaring it, so the generated API contract had no 400 and the web client is typed without that failure. PR #20 fixed consume only. The same gap exists in endpoints written before the OpenAPI types were generated (TASK-6). AGENTS.md asks every IResult endpoint to declare its response types, and Qodo has a matching rule, so these will keep coming up in reviews until fixed.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 PATCH /api/profile, POST /api/items, PUT /api/items/{id}, POST /api/shopping and GET /api/products/{barcode} declare their 400 validation problem
- [ ] #2 web/openapi.json and web/src/api-schema.d.ts are regenerated with npm run gen:api and list the 400 for each of those endpoints
- [ ] #3 No other endpoint in Endpoints.cs returns Results.ValidationProblem without declaring it
- [ ] #4 PUT /api/items/{id} declares its 409 conflict problem (a concurrent change to the item's status or quantity)
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Test first: one theory in ApiTests reads each endpoint's metadata and asserts it declares a 400 HttpValidationProblemDetails, covering every endpoint that returns Results.ValidationProblem (the five in AC #1 plus consume and the two meal-plan writes as guards).
2. Add .ProducesValidationProblem() to the five endpoints.
3. npm run gen:api; check openapi.json lists the 400s.
4. Declare the 409 conflict problem on PUT /items/{id}.
5. npm test + npm run typecheck; PR; quality gate.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
AC #1 said PUT /api/profile; the endpoint is PATCH (Endpoints.cs), so the criterion was corrected. AC #4 added from Qodo's Day 11 rerun of PR #20: only FoodItem has concurrency tokens (Status, Quantity), so PUT /items/{id} is the only undeclared write that can return 409; DELETE uses ExecuteDeleteAsync and can't.
<!-- SECTION:NOTES:END -->
