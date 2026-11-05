---
id: TASK-12
title: Merge "Gone off" and "Eat today" into one "Eat now" column on desktop
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
labels:
  - frontend
  - design
milestone: m-1
dependencies: []
priority: low
type: enhancement
ordinal: 12000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The design canvas uses one "Eat now" column on desktop; the build keeps four urgency groups (docs/decisions.md 2026-11-02). Check with the product owner first whether expired items should still stand out inside the merged column, since use-by dates may be unsafe.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Desktop inventory shows three columns with expired and today items together under "Eat now"
- [ ] #2 Expired items remain visually distinct inside the column
- [ ] #3 Phone layout is unchanged; the decisions entry is updated
<!-- AC:END -->
