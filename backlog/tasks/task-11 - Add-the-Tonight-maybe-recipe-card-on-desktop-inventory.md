---
id: TASK-11
title: Add the "Tonight, maybe?" recipe card on desktop inventory
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-19T14:52:00Z'
labels:
  - frontend
  - design
milestone: m-1
dependencies: []
priority: low
type: feature
ordinal: 11000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The design canvas shows a "Tonight, maybe?" card with the top suggestion beside the desktop inventory, nudging the user to cook before food goes off. It was left out (docs/decisions.md 2026-11-02).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Desktop inventory shows the top suggestion card, linking to Cook now
- [ ] #2 The card is hidden when there is no suggestion
- [ ] #3 Texts exist in en, nb and fi
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Share the suggestions query (useSuggestions hook) between Cook now and the inventory, so the card reuses the cached list.
2. TonightCard (features/suggestions): "Tonight, maybe?" heading, top recipe title, minutes, the items it uses, link to /cook.
3. InventoryPage: extract the urgent banner; on desktop show the banner and the card side by side; the card is desktop-only and not rendered when there is no suggestion.
4. Strings in en/nb/fi.
5. Verify: typecheck, tests, e2e for card shown + link, and card hidden with no suggestion.
<!-- SECTION:PLAN:END -->
