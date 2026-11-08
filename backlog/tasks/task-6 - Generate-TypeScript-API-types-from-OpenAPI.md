---
id: TASK-6
title: Generate TypeScript API types from OpenAPI
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-08T15:06:00Z'
labels:
  - frontend
  - backend
  - ready-for-agent
milestone: m-0
dependencies: []
priority: medium
type: chore
ordinal: 6000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
`web/src/api.ts` types are hand-written and kept in sync with `Endpoints.cs` by hand, accepted only "until the API grows" (docs/decisions.md). The meal planner and waste stats add many endpoints, so switch to the PLAN.md approach (`openapi-typescript` + `openapi-fetch`) before them. The `X-EatUp` write header and client-side error texts must keep working.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 A root `npm run gen:api` regenerates the types from the backend OpenAPI document
- [ ] #2 web code uses the generated types; no hand-written DTO types remain
- [ ] #3 Writes still send the X-EatUp header and API errors still show in the active language
- [ ] #4 AGENTS.md says to regenerate types after changing an endpoint DTO; the decisions entry is updated
<!-- AC:END -->
