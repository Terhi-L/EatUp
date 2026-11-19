---
id: TASK-10
title: Add search to the inventory
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-19T14:36:00Z'
labels:
  - frontend
  - design
milestone: m-1
dependencies: []
priority: low
type: feature
ordinal: 10000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The design canvas has a search button in the phone header and a search box on desktop; they were left out (docs/decisions.md 2026-11-02). With a full fridge, finding an item means scrolling all urgency groups.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Phone header search button and desktop search box filter the inventory by item name as you type
- [ ] #2 Search matches regardless of case and works with nb/fi names
- [ ] #3 Clearing the search restores the full list; a no-results state is shown; texts exist in en, nb and fi
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Test-first pure rule web/src/lib/search.ts nameMatches(name, query): substring, case-insensitive, NFC-normalised so composed/decomposed å/ä/ö match; å stays distinct from a; blank query matches all.
2. Search text lives in App state (shared by the phone header and the inventory page, kept while editing an item and coming back).
3. New InventorySearch.tsx: SearchField (icon, input, clear/close button, Escape clears) and PhoneHeaderSearch (search button in the phone header on / that opens the field in place of the brand).
4. InventoryPage: desktop search box next to Add food; filter by name together with the location chip; no-results message naming the search.
5. Strings in en/nb/fi; Search and Close icons in icons.tsx.
6. Verify: Vitest, typecheck, e2e test for phone and desktop search.
<!-- SECTION:PLAN:END -->
