---
id: TASK-20
title: Tell food to check from food to throw away once it is past its date
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-26T13:19:00Z'
updated_date: '2026-11-26T14:00:00Z'
labels:
  - backend
  - frontend
milestone: m-1
dependencies: []
references:
  - PLAN.md
  - docs/decisions.md
priority: medium
type: feature
ordinal: 11500
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Today everything past its date lands in "Gone off", whether it is chicken past its use-by date (unsafe to eat) or milk a day past best before (usually fine after a look, smell and taste, as Norway's "Se, lukt, smak" campaign says). So users throw away good food or keep risky food, because the app treats them alike. `PLAN.md` already planned an expiry kind (UseBy | BestBefore) on food items.

Product owner decisions (2026-11-26):
- The kind defaults from the catalogue and can be switched per item, because the package says which.
- Each best-before food has its own window in the catalogue (milk a few days, rice or pasta months). The agent proposes cautious values and the product owner approves them in the PR.
- Phone and desktop behave the same: EatUp is a phone app first, and nothing is desktop-only.
- A food the catalogue doesn't know gets a mark to check the package before throwing it out.

Unchanged unless the product owner decides otherwise: suggestions never use food past its date (`docs/decisions.md` 2026-11-02).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Each catalogue food has an expiry kind (use by or best before), and each best-before food has how many days past its date it is still worth checking; the PR lists every value for the product owner to approve
- [ ] #2 The add and edit form shows the expiry kind prefilled from the catalogue and lets the user switch it; existing items get their catalogue default through a migration that keeps their data
- [ ] #3 A use-by item past its date, or a best-before item past its window, shows under "Throw away :("
- [ ] #4 A best-before item past its date but inside its window stays with the food to eat and shows advice to look, smell and taste before eating it
- [ ] #5 A food the catalogue does not know is treated as use by: past its date it shows under "Throw away :(" with a mark to check the package first, since the user can switch it to best before
- [ ] #6 Phone and desktop show the same groups and advice, and all new text is in en, nb and fi
- [ ] #7 The flow walk covers a use-by item, a best-before item inside its window and an unknown food past their dates, at phone and desktop size
<!-- AC:END -->
