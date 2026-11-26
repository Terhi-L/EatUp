---
id: TASK-26
title: Show when an expiry date is only estimated
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-26T16:15:00Z'
labels:
  - backend
  - frontend
milestone: m-1
dependencies: []
priority: medium
type: feature
ordinal: 11670
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
When the user does not type the date from the package, EatUp fills one in from the food's typical shelf life. Once saved, nothing shows that the date was a guess, so it looks as reliable as a printed date, while the "look, smell and taste" and "Throw away :(" advice rests on it. `PLAN.md` planned an "Estimated" expiry kind; TASK-20 used the kind for use by and best before, so whether a date is estimated is stored separately. The product owner picked this change (2026-11-26); it is also the Day 14 test PR for the approval guard on database migrations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 An item remembers whether its expiry date was estimated from shelf life (the form's untouched suggestion, or the API default when no date is sent) or entered by the user
- [ ] #2 Changing the date in the edit form makes it entered, not estimated
- [ ] #3 Existing items count as entered, through a migration that keeps their data
- [ ] #4 An item with an estimated date shows that it is estimated and to check the package, on its row and its edit screen, on phone and desktop, in en, nb and fi
- [ ] #5 The flow walk shows an item with an estimated date at phone and desktop size
<!-- AC:END -->
