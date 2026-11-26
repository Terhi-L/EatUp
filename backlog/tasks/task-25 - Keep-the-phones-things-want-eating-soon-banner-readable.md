---
id: TASK-25
title: Keep the phone's "things want eating soon" banner readable
status: To Do
assignee: []
created_date: '2026-11-26T16:13:00Z'
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
