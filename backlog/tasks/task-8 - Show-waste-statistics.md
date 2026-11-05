---
id: TASK-8
title: Show waste statistics
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
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
