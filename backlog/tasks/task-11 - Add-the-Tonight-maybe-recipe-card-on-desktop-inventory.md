---
id: TASK-11
title: Add the "Tonight, maybe?" recipe card on desktop inventory
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-19T14:54:00Z'
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
- [x] #1 Desktop inventory shows the top suggestion card, linking to Cook now
- [x] #2 The card is hidden when there is no suggestion
- [x] #3 Texts exist in en, nb and fi
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Share the suggestions query (useSuggestions hook) between Cook now and the inventory, so the card reuses the cached list.
2. TonightCard (features/suggestions): "Tonight, maybe?" heading, top recipe title, minutes, the items it uses, link to /cook.
3. InventoryPage: extract the urgent banner; on desktop show the banner and the card side by side; the card is desktop-only and not rendered when there is no suggestion.
4. Strings in en/nb/fi.
5. Verify: typecheck, tests, e2e for card shown + link, and card hidden with no suggestion.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
- TonightCard (features/suggestions/TonightCard.tsx) shows the first suggestion (title, minutes, items it uses) with a "See recipe →" link to /cook. It sits beside the urgent banner, desktop only (hidden below lg); on a phone the row is hidden when it would hold only the card, so phone spacing is unchanged.
- Cook now and the inventory share one suggestions query (useSuggestions), so the card reuses the cached list and refreshes when an item is consumed.
- The urgent banner moved into its own UrgentBanner component, with no visual change.
- Strings tonight/seeRecipe in en, nb, fi (typecheck enforces same keys).
- Evidence: e2e "Tonight, maybe? card on desktop › shows the top suggestion and links to Cook now" (Eggs in 2 days → card shows Eggs, link goes to /cook) and "› is hidden when nothing needs cooking" (Milk in 10 days, after /api/suggestions responds → no card). npm run e2e 5 passed; typecheck, lint, npm test (110 xUnit, 36 Vitest) pass. Screenshots at 1280px and 390px checked.
<!-- SECTION:NOTES:END -->
