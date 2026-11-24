---
id: TASK-19
title: Plan how households share and combine their data
status: To Do
assignee: []
created_date: '2026-11-24T12:47:00Z'
labels:
  - needs-decision
  - planning
dependencies: []
priority: medium
type: docs
ordinal: 19000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The product owner wants EatUp content shared per household, used by several people (2026-11-24). Today there is one implicit local user; TASK-16 scopes data per user and TASK-17 adds households afterwards, so building accounts first would scope everything to the wrong owner. People who already use EatUp separately will also move in together, so two households (or a person and a household) need to combine without losing or doubling food, history or plans. Decide the model before TASK-16 starts.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 A written plan states which data belongs to the household (e.g. items, consume history, meal plans, shopping list) and which stays personal (e.g. language, notifications, diet and allergies)
- [ ] #2 The plan describes combining two households: duplicate items, waste history, meal plans for the same week, conflicting settings, and what each member sees afterwards
- [ ] #3 The plan says how the current single local user data becomes the first household
- [ ] #4 TASK-16 and TASK-17 are updated or split to match the plan, and the decision is logged in docs/decisions.md
<!-- AC:END -->
