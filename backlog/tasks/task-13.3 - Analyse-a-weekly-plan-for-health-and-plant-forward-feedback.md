---
id: TASK-13.3
title: Analyse a weekly plan for health and plant-forward feedback
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
labels:
  - backend
  - domain
milestone: m-3
dependencies:
  - TASK-13.1
  - TASK-13.2
parent_task_id: TASK-13
priority: medium
type: feature
ordinal: 13300
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Rule-based weekly feedback, no paid AI (PLAN.md phase 7): e.g. "no fish/omega-3 source this week" (suggest walnuts, flax, chia for vegans), "low protein on 3 days", "fast food 4x", "red meat 5x, try swapping two for legumes". Must respect diet and allergies.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 A pure domain function turns a week of meals plus profile into a list of feedback items
- [ ] #2 Unit tests cover each rule, an empty week, a vegan profile and an allergy profile
- [ ] #3 Feedback texts exist in en, nb and fi
<!-- AC:END -->
