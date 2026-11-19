---
id: TASK-10
title: Add search to the inventory
status: Done
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-19T14:47:00Z'
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
- [x] #1 Phone header search button and desktop search box filter the inventory by item name as you type
- [x] #2 Search matches regardless of case and works with nb/fi names
- [x] #3 Clearing the search restores the full list; a no-results state is shown; texts exist in en, nb and fi
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

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
- Rule web/src/lib/search.ts nameMatches, test-first (search.test.ts, 5 tests): substring, case-insensitive incl. Ø/Å/Æ/Ä/Ö, NFC-normalised so decomposed å/ä match, å/ö stay distinct from a/o, blank search matches all.
- Search text is App state shared by PhoneHeaderSearch (phone header on "/" only: search button opens the field in place of the brand; its × and Escape clear and close) and the desktop SearchField next to Add food (× and Escape clear). It survives opening an item and coming back.
- No-results message names the search (en/nb/fi). Location chips and the search combine; the urgent banner and the All count still show the whole kitchen.
- Evidence: e2e (npm run e2e, 3 passed): "the phone header search filters by name in any case and closes back to the full list" (RØM finds Rømme, kale shows the no-results text, closing restores all), "the desktop search box filters as you type and clears back to the full list" (mäm finds Mämmi only, clear restores all). Screenshots at 390px and 1280px checked: no header height jump.
- npm run typecheck, npm run lint, npm test (110 xUnit, 36 Vitest) pass.

Agent blocked on PR #17 (https://github.com/Terhi-L/EatUp/pull/17): Qodo reviews are paused because the Qodo trial has ended, so no review will arrive. Checks all pass. Asked the user: (a) wait for Qodo credits, (b) review locally instead (this PR or all PRs while Qodo is off), or (c) treat the PR as ready without a review. Reply /agent continue <answer> on the PR to resume.

User chose to continue without a Qodo review (trial ended); PR #17: https://github.com/Terhi-L/EatUp/pull/17
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
The inventory can be searched by item name: a search button in the phone header (My food only) opens a search box in place of the brand, and desktop has a search box next to Add food. Matching (web/src/lib/search.ts, unit-tested) is a case-insensitive substring match, NFC-normalised so Norwegian/Finnish letters match however they were typed, with å/ö kept distinct from a/o. Clearing (× or Escape) restores the full list; no matches shows a message in en/nb/fi. The search text lives in App state so it survives opening an item. Verified by typecheck, lint, 110 xUnit + 36 Vitest tests and two new Playwright e2e tests (phone and desktop). Merged without a Qodo review because the Qodo trial ended; the user chose to continue.
<!-- SECTION:FINAL_SUMMARY:END -->
