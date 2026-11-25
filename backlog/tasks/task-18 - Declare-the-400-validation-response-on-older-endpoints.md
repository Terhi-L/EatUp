---
id: TASK-18
title: Declare the 400 validation response on older endpoints
status: Done
assignee:
  - '@claude'
created_date: '2026-11-24T09:44:00Z'
updated_date: '2026-11-25T10:39:00Z'
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
- [x] #1 PATCH /api/profile, POST /api/items, PUT /api/items/{id}, POST /api/shopping and GET /api/products/{barcode} declare their 400 validation problem
- [x] #2 web/openapi.json and web/src/api-schema.d.ts are regenerated with npm run gen:api and list the 400 for each of those endpoints
- [x] #3 No other endpoint in Endpoints.cs returns Results.ValidationProblem without declaring it
- [x] #4 PUT /api/items/{id} declares its 409 conflict problem (a concurrent change to the item's status or quantity)
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

Review test (hackathon Day 12): AC #4 was left out of the first push on purpose, to check whether Qodo judges a PR against its Backlog task via REVIEW.md (PR #28). Round 1 on bb3158a flagged it as a Medium finding quoting criterion #4 from this file; the 409 was then added test-first (Item_writes_that_can_lose_a_race_declare_the_conflict, red for PUT before the fix).

Evidence: AC #1 Endpoints_that_validate_input_declare_the_validation_problem passes for all five (red on exactly these five before). AC #2 npm run gen:api; openapi.json responses: PATCH /api/profile 200,400 · POST /api/items 201,400 · PUT /api/items/{id} 200,400,409 · POST /api/shopping 200,201,400 · GET /api/products/{barcode} 200,400; CI API-contract check green. AC #3 every Results.ValidationProblem in Endpoints.cs (lines 76, 130, 132, 172, 258, and 288/292 via Apply for POST/PUT items) belongs to an endpoint in the same theory, which also guards consume and both meal-plan writes. AC #4 Item_writes_that_can_lose_a_race_declare_the_conflict passes (red for PUT before). 260 xUnit + 43 Vitest, typecheck and lint clean. Qodo: complete review at bb218aa, 0 open (1 Medium from round 1 implemented).
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Declared the 400 validation problem on PATCH /api/profile, POST /api/items, PUT /api/items/{id}, POST /api/shopping and GET /api/products/{barcode}, and the 409 conflict on PUT /api/items/{id}, so the generated web client is typed with both failures; openapi.json and api-schema.d.ts regenerated. Two metadata tests guard every endpoint that returns a validation problem and the item writes that can lose a race. Verified with 260 xUnit + 43 Vitest, typecheck, lint, CI and a clean Qodo review at bb218aa. Not in scope: undeclared 404s (PUT /items/{id}, GET /products) and the 502 on product lookup.
<!-- SECTION:FINAL_SUMMARY:END -->
