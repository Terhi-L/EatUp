---
id: TASK-6
title: Generate TypeScript API types from OpenAPI
status: Done
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-08T16:35:00Z'
labels:
  - frontend
  - backend
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
- [x] #1 A root `npm run gen:api` regenerates the types from the backend OpenAPI document
- [x] #2 web code uses the generated types; no hand-written DTO types remain
- [x] #3 Writes still send the X-EatUp header and API errors still show in the active language
- [x] #4 AGENTS.md says to regenerate types after changing an endpoint DTO; the decisions entry is updated
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Backend: Microsoft.AspNetCore.OpenApi plus Microsoft.Extensions.ApiDescription.Server write the OpenAPI document at build time when asked; .Produces<T>() on IResult endpoints, named DTOs for anonymous responses, strict JSON numbers.
2. Root npm run gen:api (scripts/gen-api.mjs): build in Development, then openapi-typescript via npx with pinned TypeScript 5 writes web/src/api-schema.d.ts; both generated files committed.
3. web/src/api.ts: openapi-fetch client with X-EatUp default header and the language-aware error mapping; exported types alias generated schemas.
4. AGENTS.md, GETTING_STARTED.md and docs/decisions.md updated.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
AC1: npm run gen:api exits 0 and regenerates identical files. AC2: api.ts types are aliases of components['schemas'] from api-schema.d.ts. AC3: Vitest 'send the X-EatUp header the backend requires' plus the three existing API error language tests pass; npm run e2e passes. AC4: AGENTS.md and docs/decisions.md updated. Checks: typecheck, lint, npm test (110 + 31), e2e (1) pass. Found and fixed: the OpenAPI generator ran Program.cs startup (migrate + seed) against the dev database; startup now skips that while generating. Not done: a test that fails when generated types are stale.

PR: https://github.com/Terhi-L/EatUp/pull/14
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Web API types are now generated: npm run gen:api builds the backend OpenAPI document (web/openapi.json) and runs openapi-typescript (npx, pinned TypeScript 5) into web/src/api-schema.d.ts; api.ts calls the API with openapi-fetch, sends X-EatUp by default and keeps language-aware errors. Endpoints declare real response types and 204s; JSON numbers are strict. Generation skips database setup and the reminder job, after the first run had reset a pre-migration dev database. Verified by typecheck, lint, npm test (110 + 31), npm run e2e and Qodo (no open findings). Follow-up idea: a test that fails when the generated types are stale.
<!-- SECTION:FINAL_SUMMARY:END -->
