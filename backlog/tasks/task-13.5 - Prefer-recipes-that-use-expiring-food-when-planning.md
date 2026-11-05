---
id: TASK-13.5
title: Prefer recipes that use expiring food when planning
status: To Do
assignee: []
created_date: '2026-11-05T13:02:00Z'
labels:
  - backend
  - frontend
  - domain
milestone: m-3
dependencies:
  - TASK-13.4
parent_task_id: TASK-13
priority: medium
type: feature
ordinal: 13500
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The planner should help eat food before it goes off (PLAN.md phase 7): when picking a recipe for a day, recipes using items that will be urgent by that day rank first, reusing the existing suggestion ranking.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Recipe choices for a planned day rank by the items expiring by that day, respecting diet and allergies
- [ ] #2 Domain tests show an item expiring Wednesday boosts recipes on Monday-Wednesday but not later days
<!-- AC:END -->
