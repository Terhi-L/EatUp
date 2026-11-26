---
id: TASK-26
title: Show when an expiry date is only estimated
status: Done
assignee:
  - '@claude'
created_date: '2026-11-26T16:15:00Z'
updated_date: '2026-11-26T17:06:00Z'
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
- [x] #1 An item remembers whether its expiry date was estimated from shelf life (the form's untouched suggestion, or the API default when no date is sent) or entered by the user
- [x] #2 Changing the date in the edit form makes it entered, not estimated
- [x] #3 Existing items count as entered, through a migration that keeps their data
- [x] #4 An item with an estimated date shows that it is estimated and to check the package, on its row and its edit screen, on phone and desktop, in en, nb and fi
- [x] #5 The flow walk shows an item with an estimated date at phone and desktop size
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Domain, test first: Expiry.DateIsEstimated(sentDate, sentAsSuggestion): estimated when no date is sent (the API fills in the shelf-life default) or the form sends its untouched suggestion.
2. FoodItem.ExpiryEstimated, ItemInput.ExpiryEstimated (default false) and ItemDto.ExpiryEstimated; Apply uses the domain rule. API tests first.
3. Migration AddExpiryEstimated (default false = entered); upgrade test: existing items count as entered and keep their data. npm run gen:api.
4. Web: lib helper for what the form sends (new item: the untouched suggestion; existing item: keeps its estimate until the date changes), Vitest first; the row shows 'Estimated date: check the package'; the edit screen keeps the shelf-life hint for a kept estimate; en/nb/fi.
5. Flow walk step for an item with an estimated date; checks; Qodo local review; PR.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Flow walk (npm run walk, phone Pixel 7 + desktop 1440x900, 6/6 passed; screenshots in web/flow-walk-results/):
| Step | Phone | Desktop | Proves |
|---|---|---|---|
| kitchen, add-milk, milk-added, cook-now, eggs-eaten, profile-stats | OK | OK | main loop unchanged; milk added through the untouched suggestion now shows the estimated line (AC1) |
| estimated-row | OK | OK | AC1, AC4: carrots added without a date say 'Estimated date: check the package' |
| estimated-edit | OK | OK | AC4: the edit screen keeps the shelf-life hint, with 'Matches the package' |
| entered-row | OK | OK | AC2: a new date makes it entered |
| matches-package | OK | OK | confirming a date that matches the package makes it entered |
| past-date-* | OK apart from the known TASK-24 hint and TASK-25 banner | OK apart from TASK-24 | - |
Qodo local review: round 1 two Mediums. 'Generated type makes the flag required' dismissed as false_positive with the product owner (regenerating gives no diff; openapi-typescript makes defaulted fields required). 'Verified package dates stay marked estimated' fixed: any edit of the date or the new 'Matches the package' button makes it entered (product owner chose this). Round 2: resolved, nothing new; its comment pointed at the barcode-scan reset keeping the confirmed flag, fixed with a reset (no test: there are no component tests and the walk can't scan).

Evidence: AC1 ExpiryTests.A_date_is_estimated_when_it_comes_from_shelf_life_rather_than_the_user and ApiTests.An_item_remembers_whether_its_date_was_estimated_until_a_date_is_entered; AC2 the same API test (PUT with a new date) plus expiryEstimate.test.ts and the walk's entered-row step; AC3 migration 20261126161710_AddExpiryEstimated (default false) and DatabaseSetupTests.Upgrading_counts_existing_dates_as_entered_and_keeps_item_data; AC4 walk screenshots at both sizes, texts in en/nb/fi (i18n key test passes); AC5 walk 6/6. dotnet test 292, Vitest 57, e2e 12, typecheck and oxlint clean; gen:api regenerated.
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Items now remember whether their expiry date was only estimated from typical shelf life (the API default when no date is sent, or the form's untouched suggestion). The row says 'Estimated date: check the package', and the edit screen keeps the shelf-life hint with a 'Matches the package' button; editing the date or confirming it makes it entered. The rule lives in the domain (Expiry.DateIsEstimated); a migration adds Items.ExpiryEstimated, and existing items count as entered. Verified with 292 xUnit, 57 Vitest, 12 e2e, the walk at phone and desktop size, typecheck and lint. Also the Day 14 test PR for the approval guard on migrations.
<!-- SECTION:FINAL_SUMMARY:END -->
