---
id: TASK-7
title: Record every consume as a waste-history event
status: Done
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-12T14:21:00Z'
labels:
  - backend
  - domain
milestone: m-2
dependencies:
  - TASK-4
priority: medium
type: feature
ordinal: 7000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Waste stats (TASK-8) need how much of each item was used or thrown away, and when. Today a partial "thrown away" only lowers the quantity and is not recorded (docs/decisions.md), and a full consume only flips the status, so history is lost or ambiguous. Deleting an item hard-deletes it (Endpoints.cs DELETE /items/{id}, DemoData.ResetItemsAsync), so history must not depend on the item row surviving.

Out of scope: quantity reductions made through the edit form (PUT /items/{id}).

Open questions for the product owner:
1. Backfill old Used/Discarded items, or start history empty?
2. Should lowering quantity in the edit form count as "used", or stay invisible?
3. Should demo reset also clear events (clean demo) or keep them (real history)?
4. Is 409 right for re-consuming a non-active item, or should it be a silent no-op?
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Every consume (used or discarded, partial or full) stores a ConsumeEvent with item id (nullable), item name, ingredient key, amount, unit, outcome and a UTC timestamp from TimeProvider
- [x] #2 The recorded amount is what was actually consumed: the partial amount, or the remaining quantity for a full or overshooting consume
- [x] #3 Deleting an item or resetting demo items keeps its events (FK set to null)
- [x] #4 Consuming an item that is not Active returns 409 and records nothing
- [x] #5 Existing partial-use behaviour (quantity drops, item stays active) is unchanged
- [x] #6 The consume rule lives in Domain/ with unit tests written first; endpoint tests cover partial, full, overshoot and already-consumed cases
- [x] #7 A migration adds the table and backfills one event per existing Used/Discarded item from StatusChangedAt
- [x] #8 The decisions entry about unrecorded partial discards is updated
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Domain/Consumption: CanConsume + Consume returning a ConsumeEvent snapshot (tests first).
2. ConsumeEvent entity with nullable FoodItemId FK ON DELETE SET NULL.
3. Consume endpoint: 409 for non-active items, store the event.
4. Migration AddConsumeEvents with backfill of Used/Discarded items.
5. Endpoint + migration tests, gen:api, decisions entry.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
No index on ConsumedAt: EF SQLite cannot translate DateTimeOffset comparisons, so TASK-8 should decide how it queries by date. Verified with npm test (121 xUnit, 31 Vitest) and npm run typecheck.
<!-- SECTION:NOTES:END -->

## Comments

<!-- COMMENTS:BEGIN -->
created: 2026-11-10T13:16:00Z

---

Revised after a Qodo Codebase Wisdom investigation (main @ 29d035a). Original title: "Record partial and full discards as waste history". Original AC: (1) every consume stores an event with item, amount, unit, outcome and date; (2) partial use unchanged; (3) domain tests first, endpoint tests cover partial and full discards; (4) decisions entry updated.

Why it changed: DELETE /items and demo reset hard-delete items (Endpoints.cs:112, DemoData.cs:19), so events need a snapshot and nullable FK; consume has no Active check (Endpoints.cs:117) so re-consuming would double-count; overshoot amounts become full consumes (Endpoints.cs:121) so the event must store the remaining quantity; consume logic lives in the endpoint, so AC "domain tests first" means extracting a Domain rule; StatusChangedAt allows a backfill. cross-repo relations: none. Prior-PR search failed upstream (MT-UPSTREAM-DOWN), not checked.

---

author: @claude
created: 2026-11-12T14:18:00Z

---

Product owner decisions (2026-11-12): backfill existing Used/Discarded items (AC #7); edit-form quantity changes stay out of scope; demo reset keeps events with item id set to null; re-consuming a non-active item returns 409.

---
<!-- COMMENTS:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Every consume now stores a ConsumeEvent (item snapshot, actual amount, unit, outcome, UTC time). The rule moved to Domain/Consumption with unit tests; the endpoint returns 409 for items that are no longer active. Events keep a nullable item FK (ON DELETE SET NULL), so deletes and demo resets keep history. Migration AddConsumeEvents backfills one full event per existing Used/Discarded item. Tests cover partial, full, overshoot, re-consume, delete/reset and the backfill. Follow-up for TASK-8: SQLite stores DateTimeOffset as text, so date filtering needs care.
<!-- SECTION:FINAL_SUMMARY:END -->
