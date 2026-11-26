---
id: TASK-23
title: >-
  Make the flow walk worth running: skip non-screen changes, start from demo
  data, show the steps on one page
status: To Do
assignee: []
created_date: '2026-11-26T15:01:00Z'
labels:
  - frontend
milestone: m-1
dependencies: []
priority: medium
type: enhancement
ordinal: 11550
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The first real flow walk (TASK-20, PR #32) passed and confirmed the main loop at phone and desktop size, but it missed all three High findings that Qodo's full review found afterwards. One of them, yesterday's yoghurt shown under "Throw away :(" on first run, sits on the very first screen a new user sees; the walk never shows that screen because it seeds its own data. The product owner also could not see the steps: the screenshots sit in a local folder, and the walk's .ts file opens as a video on Windows. The product owner wants all three improvements (2026-11-26).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 The flow-walk skill says when to walk (a change to anything a user sees) and when to skip it (backend-only or docs changes), and the agent states which applies before it starts
- [ ] #2 A walk step starts from the demo data a new user gets (after a demo reset) and screenshots the kitchen at phone and desktop size
- [ ] #3 Every walk writes one page, web/flow-walk-results/index.html, that shows each step's phone and desktop screenshots side by side under the step name, and opens in a browser without a server
- [ ] #4 The skill's report gives the user the path to that page
<!-- AC:END -->
