---
id: TASK-21
title: Show eaten as a knife and fork instead of a check mark
status: Done
assignee:
  - '@claude'
created_date: '2026-11-26T13:31:00Z'
updated_date: '2026-11-26T16:14:00Z'
labels:
  - frontend
  - design
milestone: m-1
dependencies: []
priority: low
type: enhancement
ordinal: 11600
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The inventory row has a green check mark that records the whole item as eaten (it counts in "eaten this month" on Profile), next to a bin that records it as thrown out (waste). The product owner did not know what the check mark meant (2026-11-26): a check reads as "done" or "OK", not as "eaten". A knife and fork next to the bin reads as "eaten or thrown out".
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 The eaten button on each inventory row shows a knife-and-fork icon instead of the check mark, on phone and desktop
- [x] #2 The eaten buttons on the edit screen use the same icon
- [x] #3 The flow walk shows the new icon at phone and desktop size
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. KnifeAndForkIcon in web/src/icons.tsx; use it for the eaten button on inventory rows and both eaten buttons on the edit screen (CheckIcon stays for the notifications 'enabled' line).
2. Flow walk (skill): add a step that scrolls to the edit screen's eaten buttons and checks they aren't covered by the navigation.
3. npm test, typecheck, lint, e2e, walk; Qodo local review; PR.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Flow walk (npm run walk, phone Pixel 7 + desktop 1440x900, 6/6 passed; screenshots in web/flow-walk-results/):
| Step | Phone | Desktop | Proves |
|---|---|---|---|
| kitchen | OK | OK | AC1: knife and fork on every row |
| add-milk, milk-added, cook-now, eggs-eaten, profile-stats | OK | OK | main loop unchanged |
| past-date-groups, kimchi-best-before | OK | OK | edit screen eaten buttons (desktop shows both) |
| past-date-kind-from-catalogue | hint under an empty date for Pasta in the fridge (older bug, TASK-24) | same | - |
| past-date-kimchi-to-check | urgent banner cut with an ellipsis (older bug, TASK-25) | OK | - |
| eaten-buttons (new) | OK | OK | AC2, AC3: both eaten buttons show the icon and aren't covered by the navigation |
New walk step: on the phone, a full-page capture drew the bottom bar over the edit screen's 'Used some of it?' button, so the step scrolls to the bottom like a user and checks the buttons' boxes don't overlap the visible main navigation (red without the scroll: 'covered by the navigation'). Product owner decisions: keep the icon (looks fine on the phone); distinct labels for the two eaten buttons go to TASK-22; the two older bugs became TASK-24 and TASK-25.

Evidence: AC1 kitchen screenshots phone + desktop show the knife and fork on every row; AC2 desktop past-date-kimchi-best-before and phone/desktop eaten-buttons show both edit-screen eaten buttons with the icon, and the walk asserts an icon in each; AC3 walk 6/6. dotnet test 287, Vitest 52, e2e 12, typecheck and oxlint clean. Qodo local review complete: 1 Low (the covered check compares against the navigation only), flagged for the product owner, left unchanged.
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
The eaten buttons now show a knife and fork instead of a check mark, on inventory rows and on both edit-screen eaten buttons, because a check read as 'done', not 'eaten'. The flow walk gained a step that scrolls to the edit screen's eaten buttons and checks no navigation covers them. Verified with 287 xUnit, 52 Vitest, 12 e2e, the walk at phone and desktop size, typecheck and lint. Follow-ups: TASK-24 and TASK-25 (older bugs the walk found), distinct labels for the two eaten buttons in TASK-22.
<!-- SECTION:FINAL_SUMMARY:END -->
