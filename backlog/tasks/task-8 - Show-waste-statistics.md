---
id: TASK-8
title: Show waste statistics
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-24T11:45:00Z'
labels:
  - frontend
  - backend
milestone: m-2
dependencies:
  - TASK-7
priority: medium
type: feature
ordinal: 8000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The point of EatUp is less food waste, but users never see whether it is working. PLAN.md phase 8 asks for stats from used vs discarded history, e.g. "you saved 14 items this month".
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 An endpoint returns used vs discarded counts for the current and previous month, in the user time zone
- [ ] #2 A screen or Profile section shows them with a short encouraging line; texts exist in en, nb and fi
- [ ] #3 With no history the screen shows an empty state, not zeros that look like failure
- [ ] #4 Domain and endpoint tests cover month boundaries and time zones
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Domain/WasteStats (tests first): local month of each event in the user's time zone (Expiry.LocalToday), counts of items touched per outcome for this and last month, HasHistory.
2. GET /api/stats/waste: loads events, returns this/last month counts and HasHistory; endpoint tests for month boundaries and time zones with a fixed TimeProvider.
3. Web: api.wasteStats(), a 'Your food' section on Profile with this and last month, an encouraging line chosen by a pure function (Vitest), and an empty state when there is no history. Texts in en, nb and fi.
4. gen:api, checks, Qodo local review (rule 7), PR.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Ticket check (Codebase Wisdom, main af57ebb): history is ConsumeEvents (PR #20, migration 20261112142951_AddConsumeEvents) with one event per use/discard action: FoodItemId (null after delete), ItemName, Amount, Unit, Outcome, ConsumedAt (UTC DateTimeOffset). Partial uses create several events per item and units are mixed, so 'counts' was ambiguous. Product owner decision (2026-11-24): count items touched, each item once per outcome per month (an item partly used and partly thrown away counts in both); events of deleted items count one each. Backfilled history (one event per item finished before 2026-11-12, dated StatusChangedAt) counts the same way. Profile page (web/src/features/profile/ProfilePage.tsx) is a list of sections, so the stats become a Profile section.
<!-- SECTION:NOTES:END -->
