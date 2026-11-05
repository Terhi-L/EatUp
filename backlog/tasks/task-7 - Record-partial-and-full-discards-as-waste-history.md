---
id: TASK-7
title: Record partial and full discards as waste history
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
labels:
  - backend
  - domain
milestone: m-2
dependencies:
  - TASK-4
priority: medium
type: feature
ordinal: 7000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Waste stats need to know how much of each item was used or thrown away, and when. Today a partial "thrown away" only lowers the quantity and is not recorded as waste (docs/decisions.md 2026-11-02), and a full consume only flips the item status, so history is lost or ambiguous.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Every consume (used or discarded, partial or full) stores an event with item, amount, unit, outcome and date
- [ ] #2 Existing behaviour of partial use (quantity drops, item stays active) is unchanged
- [ ] #3 Domain rule tests come first; endpoint tests cover partial and full discards
- [ ] #4 The decisions entry about unrecorded partial discards is updated
<!-- AC:END -->
