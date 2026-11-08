---
id: TASK-13.1
title: Tag recipes with nutrition facts
status: To Do
assignee: []
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-08T15:06:00Z'
labels:
  - backend
  - domain
  - ready-for-agent
milestone: m-3
dependencies: []
parent_task_id: TASK-13
priority: medium
type: feature
ordinal: 13100
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The weekly health analysis needs to know per recipe: protein source, whether it has fish/omega-3, vegetable portions, and a fast-food/ultra-processed flag (PLAN.md phase 7). Recipes live in `backend/src/EatUp.Api/Seed/catalog.json`.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Every seed recipe has the nutrition tags, and a test fails if one is missing
- [ ] #2 Tags are read into the domain model with no EF or HTTP dependencies
<!-- AC:END -->
