---
id: TASK-21
title: Show eaten as a knife and fork instead of a check mark
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-26T13:31:00Z'
updated_date: '2026-11-26T15:57:00Z'
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
- [ ] #1 The eaten button on each inventory row shows a knife-and-fork icon instead of the check mark, on phone and desktop
- [ ] #2 The eaten buttons on the edit screen use the same icon
- [ ] #3 The flow walk shows the new icon at phone and desktop size
<!-- AC:END -->
