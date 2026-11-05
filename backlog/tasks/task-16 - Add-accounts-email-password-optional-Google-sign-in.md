---
id: TASK-16
title: 'Add accounts: email + password, optional Google sign-in'
status: To Do
assignee: []
created_date: '2026-11-05T13:02:00Z'
labels:
  - backend
  - frontend
  - needs-decision
milestone: m-4
dependencies:
  - TASK-4
priority: low
type: feature
ordinal: 16000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The MVP has a single implicit local user (docs/decisions.md 2026-11-02). Households and any shared use need accounts. PLAN.md § 3 and phase 1 describe the target: ASP.NET Core Identity with HttpOnly SameSite=Strict cookies, optional Google sign-in behind `IGoogleTokenValidator`. Confirm with the product owner before starting, since it changes the zero-setup local experience.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Users can register, log in and log out with email + password; Google sign-in appears only when a client ID is configured
- [ ] #2 Items, profile, push subscriptions and meal plans are scoped to the user, and the existing single-user data migrates to a first account
- [ ] #3 Integration tests prove users cannot see each other data
- [ ] #4 GETTING_STARTED.md documents the demo login; decisions are logged
<!-- AC:END -->
