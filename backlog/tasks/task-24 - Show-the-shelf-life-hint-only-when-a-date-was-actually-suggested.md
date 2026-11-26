---
id: TASK-24
title: Show the shelf-life hint only when a date was actually suggested
status: To Do
assignee: []
created_date: '2026-11-26T16:13:00Z'
labels:
  - frontend
milestone: m-1
dependencies: []
priority: low
type: bug
ordinal: 11650
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The add form shows "Suggested from typical shelf life. Check the package!" whenever the name matches a catalogue food, even when that food has no shelf life for the chosen place, so the date stays empty (for example Pasta with Fridge selected). The hint then points at a suggestion that is not there. Found by the flow walk while building TASK-21 (2026-11-26); older than that change.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 The hint shows only while the form holds a date it suggested from shelf life
- [ ] #2 A matched food with no shelf life for the chosen place leaves the date empty and shows no hint
- [ ] #3 Tests cover both cases
<!-- AC:END -->
