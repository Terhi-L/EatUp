---
id: TASK-12
title: >-
  Group food as Throw away, Eat now, Within 2 days and Later on phone and
  desktop
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-26T13:19:00Z'
labels:
  - frontend
  - design
milestone: m-1
dependencies:
  - TASK-20
priority: low
type: enhancement
ordinal: 12000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The design canvas merges "Gone off" and "Eat today" into one "Eat now" column on desktop; the build keeps four urgency groups (`docs/decisions.md` 2026-11-02). Once TASK-20 splits food past its date into food to check and food to throw away, "Eat now" can hold everything worth eating today.

Product owner decision (2026-11-26): nothing is desktop-only. EatUp is a phone app first, and a grouping that only works on desktop points to a gap in the phone design, so phone and desktop get the same groups.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Phone and desktop show the same groups: "Throw away :(", "Eat now", "Within 2 days" and "Later"
- [ ] #2 "Eat now" holds food for today and best-before food still worth checking, which keeps its look, smell and taste advice
- [ ] #3 "Throw away :(" stands apart visually from the food to eat
- [ ] #4 The flow walk shows every group at phone and desktop size, and `docs/decisions.md` is updated
<!-- AC:END -->
