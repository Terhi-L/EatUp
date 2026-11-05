---
id: TASK-10
title: Add search to the inventory
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
labels:
  - frontend
  - design
milestone: m-1
dependencies: []
priority: low
type: feature
ordinal: 10000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The design canvas has a search button in the phone header and a search box on desktop; they were left out (docs/decisions.md 2026-11-02). With a full fridge, finding an item means scrolling all urgency groups.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Phone header search button and desktop search box filter the inventory by item name as you type
- [ ] #2 Search matches regardless of case and works with nb/fi names
- [ ] #3 Clearing the search restores the full list; a no-results state is shown; texts exist in en, nb and fi
<!-- AC:END -->
