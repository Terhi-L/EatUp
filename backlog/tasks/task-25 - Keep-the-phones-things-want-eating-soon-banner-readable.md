---
id: TASK-25
title: Keep the phone's "things want eating soon" banner readable
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-26T16:13:00Z'
updated_date: '2026-11-26T18:30:00Z'
labels:
  - frontend
milestone: m-1
dependencies: []
priority: low
type: bug
ordinal: 11660
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
At phone width the banner listing food that wants eating soon cuts its line with an ellipsis ("Kimchi, Yoghurt, Bread and 1 ..."), so the "and N more" part is lost. Found by the flow walk while building TASK-21 (2026-11-26); older than that change.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 At phone width the banner shows every listed name and the "and N more" text in full, wrapping instead of clipping
- [ ] #2 The flow walk shows the banner with four urgent items at phone size
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Test first: a flow-walk test with four urgent items checks the banner's names line isn't clipped (scrollWidth <= clientWidth) at phone and desktop size.
2. Let the names line wrap (break-words) instead of truncate.
3. Checks; Qodo local review; PR.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Flow walk (npm run walk, phone Pixel 7 + desktop 1440x900, 10/10 passed):
| Step | Phone | Desktop | Proves |
|---|---|---|---|
| urgent-banner (new) | OK: 'Yoghurt, Bread, Mushrooms and / 1 more' wraps | OK: one line | AC1, AC2 |
| past-date-kimchi-to-check | OK: the banner that showed the bug now wraps | OK | AC1 |
| all other steps (main loop, estimated dates, past date, eaten buttons) | OK apart from the known TASK-24 hint | OK apart from TASK-24 | unchanged |
The new test failed on the phone before the fix ('names cut off') and passed on desktop. Design note for the product owner: on the phone the wrap can leave 'more' alone on the second line. Qodo local review round 1: 1 Medium (the walk seeded five items, the ticket says four) fixed: four items, red re-proved.
<!-- SECTION:NOTES:END -->
