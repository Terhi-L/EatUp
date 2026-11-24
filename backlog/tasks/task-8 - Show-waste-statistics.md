---
id: TASK-8
title: Show waste statistics
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-24T12:19:00Z'
labels:
  - frontend
  - backend
milestone: m-2
dependencies:
  - TASK-7
priority: medium
type: feature
ordinal: 8000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The point of EatUp is less food waste, but users never see whether it is working. PLAN.md phase 8 asks for stats from used vs discarded history, e.g. "you saved 14 items this month".
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 An endpoint returns used vs discarded counts for the current and previous month, in the user time zone
- [x] #2 A screen or Profile section shows them with a short encouraging line; texts exist in en, nb and fi
- [x] #3 With no history the screen shows an empty state, not zeros that look like failure
- [x] #4 Domain and endpoint tests cover month boundaries and time zones
- [x] #5 Counts are items touched: each item once per outcome per month, so an item partly used and partly thrown away counts in both
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Domain/WasteStats (tests first): local month of each event in the user's time zone (Expiry.LocalToday), counts of items touched per outcome for this and last month, HasHistory.
2. GET /api/stats/waste: loads events, returns this/last month counts and HasHistory; endpoint tests for month boundaries and time zones with a fixed TimeProvider.
3. Web: api.wasteStats(), a 'Your food' section on Profile with this and last month, an encouraging line chosen by a pure function (Vitest), and an empty state when there is no history. Texts in en, nb and fi.
4. gen:api, checks, Qodo local review (rule 7), PR.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Ticket check (Codebase Wisdom, main af57ebb): history is ConsumeEvents (PR #20, migration 20261112142951_AddConsumeEvents) with one event per use/discard action: FoodItemId (null after delete), ItemName, Amount, Unit, Outcome, ConsumedAt (UTC DateTimeOffset). Partial uses create several events per item and units are mixed, so 'counts' was ambiguous. Product owner decision (2026-11-24): count items touched, each item once per outcome per month (an item partly used and partly thrown away counts in both); events of deleted items count one each. Backfilled history (one event per item finished before 2026-11-12, dated StatusChangedAt) counts the same way. Profile page (web/src/features/profile/ProfilePage.tsx) is a list of sections, so the stats become a Profile section.

Evidence per AC (npm test: 250 xUnit + 43 Vitest; Playwright 11/11; typecheck + oxlint clean):
#1 GET /api/stats/waste: WasteStatsApiTests.Counts_this_and_last_month_in_the_users_time_zone, Month_edges_count_exactly_and_older_history_still_counts_as_history.
#2 Profile section 'Your food' with encouraging line, en/nb/fi: web/src/features/profile/WasteStatsSection.tsx, wasteStats.test.ts (encouragementFor, monthName in three locales), e2e 'count food eaten this month'. nb/fi texts await the product owner's native-speaker review.
#3 Empty state: WasteStatsApiTests.Without_history_the_stats_say_so, e2e 'explain there is nothing yet instead of showing zeros'.
#4 Month boundaries and time zones: WasteStatsTests.Months_follow_the_users_time_zone_not_UTC, In_January_last_month_is_December_of_the_previous_year, Loading_starts_no_later_than_the_first_moment_of_last_month (Oslo, UTC+14, UTC-11), plus the API tests above.
#5 Items touched: WasteStatsTests.An_item_used_in_several_parts_counts_once, An_item_partly_used_and_partly_thrown_away_counts_in_both, WasteStatsApiTests.Deleting_an_item_keeps_its_count.
Qodo pre-commit review (quality gate): round 1 = 1 High + 3 Medium, round 2 = 2 new Medium, round 3 = none open; the security finding was dismissed as a risk the product owner accepted (same exposure as GET /api/items; accounts in TASK-16). Planning agent checked the High and shared-code fixes.
<!-- SECTION:NOTES:END -->
