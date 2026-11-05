---
id: TASK-14
title: Read name and expiry date from a label photo
status: To Do
assignee: []
created_date: '2026-11-05T13:02:00Z'
labels:
  - frontend
  - needs-decision
milestone: m-4
dependencies: []
priority: low
type: feature
ordinal: 14000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Typing expiry dates is the slowest part of adding food (PLAN.md phase 6). The obvious AI option (Claude vision) costs money, so the product owner must choose before work starts: free local Tesseract.js OCR in the browser plus a date parser, or a paid option behind an `ILabelReader` interface. Rule 5 of AGENTS.md applies.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 The product owner has chosen the approach and it is logged in docs/decisions.md
- [ ] #2 A photo taken via the camera input is resized client-side and prefills name and expiry in the add form; nothing is saved without confirmation
- [ ] #3 The date parser handles common formats (e.g. "BEST BEFORE 12.11.26", "Use by 12 NOV", "Parasta ennen", "Best før") with unit tests
<!-- AC:END -->
