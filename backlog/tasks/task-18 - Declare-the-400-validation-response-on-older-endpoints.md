---
id: TASK-18
title: Declare the 400 validation response on older endpoints
status: To Do
assignee: []
created_date: '2026-11-24T09:44:00Z'
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
- [ ] #1 PUT /api/profile, POST /api/items, PUT /api/items/{id}, POST /api/shopping and GET /api/products/{barcode} declare their 400 validation problem
- [ ] #2 web/openapi.json and web/src/api-schema.d.ts are regenerated with npm run gen:api and list the 400 for each of those endpoints
- [ ] #3 No other endpoint in Endpoints.cs returns Results.ValidationProblem without declaring it
<!-- AC:END -->
