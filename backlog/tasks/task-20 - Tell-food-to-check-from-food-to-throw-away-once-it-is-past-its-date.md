---
id: TASK-20
title: Tell food to check from food to throw away once it is past its date
status: Done
assignee:
  - '@claude'
created_date: '2026-11-26T13:19:00Z'
updated_date: '2026-11-26T15:17:00Z'
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
- [x] #1 Each catalogue food has an expiry kind (use by or best before), and each best-before food has how many days past its date it is still worth checking; the PR lists every value for the product owner to approve
- [x] #2 The add and edit form shows the expiry kind prefilled from the catalogue and lets the user switch it; existing items get their catalogue default through a migration that keeps their data
- [x] #3 A use-by item past its date, or a best-before item past its window, shows under "Throw away :("
- [x] #4 A best-before item past its date but inside its window stays with the food to eat and shows advice to look, smell and taste before eating it
- [x] #5 A food the catalogue does not know is treated as use by: past its date it shows under "Throw away :(" with a mark to check the package first, since the user can switch it to best before
- [x] #6 Phone and desktop show the same groups and advice, and all new text is in en, nb and fi
- [x] #7 The flow walk covers a use-by item, a best-before item inside its window and an unknown food past their dates, at phone and desktop size
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Domain (tests first): ExpiryKind {UseBy, BestBefore} and ExpiryAdvice {None, LookSmellTaste, CheckPackage}; catalogue Ingredient gets a required expiryKind and, for best-before food, checkDaysPastBestBefore per storage location (a place missing = no extra days, so cooked rice in the fridge is not 'look, smell, taste'); Catalog rejects best-before food without a window and use-by food with one. Expiry.FreshnessOf(item, ingredient, today) returns effective expiry, days left, urgency and advice: best-before past its date inside its window = Today + LookSmellTaste; past the window or use by = Expired; unknown food (no catalogue entry) use by past its date = Expired + CheckPackage. No window when the opened shelf life decided the date; best-before food the catalogue has no window for (unknown, or switched from use by) gets a 2-day fallback.
2. Data: FoodItem.ExpiryKind stored as text; migration AddExpiryKind adds it (default UseBy) and sets BestBefore on existing items whose catalogue food is best before; upgrade test checks every catalogue key plus an unknown item keep their data.
3. API: ItemInput.ExpiryKind optional (null = catalogue default, else use by; undefined value = 400); ItemDto gains ExpiryKind and Advice; npm run gen:api.
4. Catalogue: cautious kind + window values for all 38 foods, listed in the PR for the product owner.
5. Web: 'Gone off' becomes 'Throw away :(' (en/nb/fi); form gets a 'The package says' Use by / Best before toggle prefilled from the catalogue match; inventory rows show 'Best before yesterday' badge + look/smell/taste advice, or the check-the-package mark; all text in en, nb, fi.
6. Flow walk: kitchen gets a use-by item past its date so every group still shows; new test walks use-by, best-before-in-window and unknown food past their dates, the prefilled kind, and switching the unknown food to best before, at phone and desktop.
7. docs/decisions.md entry; npm test, npm run typecheck, lint, e2e; Qodo local review (quality gate); PR.

Revised after Qodo review (product owner's choice 2026-11-26): the form sends a kind only for a tap on the current name or an item that keeps its name (case-insensitive); otherwise the backend picks the default of the food it matches (web/src/lib/expiryKind.ts).
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented domain (Expiry.FreshnessOf, ExpiryKind/ExpiryAdvice, catalogue windows per storage place with validation), AddExpiryKind migration with upgrade test over every catalogue key, API input/DTO + regenerated types, form toggle, inventory badge/advice, en/nb/fi text, decisions.md entry. npm test (279 xUnit + 45 Vitest), typecheck, lint, e2e (12) and flow walk (4) pass. Builds used a redirected output folder because npm run dev held bin/.

Qodo local review: round 1 (full) raised High 5f6aa18a 'Edited chicken gets unsafe eating advice' and Medium 18e2feca 'Form decides the default expiry kind in the browser'; a planning agent checked the fix plan (caught click-then-rename and trailing spaces, and that sending null makes loose matches like 'Egg salad' best before). Product owner chose to let the backend decide: people throw away good food far more often than the opposite. Round 2 resolved both and raised Medium 4f8c2d39 (case-only rename reset a chosen kind), fixed. Round 3 at fbe22fd: no open findings, coverage complete.

Flow walk (npm run walk, phone + desktop; screenshots in web/flow-walk-results/): kitchen OK/OK; add-milk OK/OK (Save under the bottom bar only in the phone full-page capture, reachable); milk-added, cook-now, eggs-eaten, profile-stats OK/OK; past-date-groups OK/OK (AC3-6); past-date-kind-from-catalogue OK/OK (AC2); past-date-kimchi-best-before OK/OK and past-date-kimchi-to-check OK/OK (AC5). Kitchen seed gained Chicken -1; no existing step changed. Finnish phone check: long chip text wraps.

PR #32 hosted Qodo review (extended, complete at 41e4ef2) raised 3 High and 2 Medium. Fixed (plan checked by a planning agent): demo items now get their catalogue kind (3cd47d0d); the best-before window is capped at the opened limit instead of only skipped when opening decides the date (96ec06c4, decisions.md updated); Expiry.DefaultKind holds the default kind (6b2fb99a); sameName uses the catalogue's word normalisation, so hyphens and extra spaces don't reset a chosen kind (1984364c). Dismissed as intentional on the product owner's decision: 5f633b04 'Users see use by but save best before'. Checks after the fixes: 287 xUnit + 52 Vitest, typecheck, lint, e2e 12, walk 4 pass.
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Food past its date is now split by expiry kind. Each catalogue food is use by or best before, and best-before food has cautious days worth checking per storage place (listed in the PR for approval; none for cooked rice in the fridge). Items store their kind: the backend picks the catalogue default unless the user taps one in the form; the AddExpiryKind migration gave existing items theirs. Use-by food, or best-before food past its window, shows under 'Throw away :('; best-before food inside its window stays under 'Eat today' with a 'Best before yesterday' badge and look/smell/taste advice; unknown food counts as use by and asks to check the package. Opened food gets no window, and best-before food without a catalogue window gets 2 days. Suggestions and the reminder still skip food past its date. Verified: 280 xUnit + 50 Vitest, typecheck, lint, 12 e2e and the flow walk at phone and desktop; Qodo local review clean after 3 rounds. Follow-ups: product owner approves the catalogue values in the PR; nb/fi texts await native review; TASK-12 renames 'Eat today' to 'Eat now'.
<!-- SECTION:FINAL_SUMMARY:END -->
