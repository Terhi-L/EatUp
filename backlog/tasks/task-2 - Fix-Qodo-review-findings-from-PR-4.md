---
id: TASK-2
title: 'Fix Qodo review findings from PR #4'
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-04T21:08:00Z'
labels:
  - backend
  - frontend
  - i18n
dependencies: []
type: bug
ordinal: 2000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Qodo reviewed PR #4 (Norwegian/Finnish, barcode scanning, Cupboard) and found bugs. Triage comment on PR #4 (issuecomment-6642134390) lists what is fixed here; #1 (DB migration) and #6 (concurrent language picks) are out of MVP scope.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Search for milk/butter also offers Coconut milk/Peanut butter (multi-word names match by word)
- [ ] #2 Ticking opened before the ingredient lookup returns still moves a tin to the fridge, unless the user picked a location
- [ ] #3 Typing an English name (e.g. Chickpeas) with a Norwegian/Finnish profile gets the shelf-life suggestion
- [ ] #4 A failed language save restores the previous UI language
- [ ] #5 PATCH /profile rejects an undefined language with 400, and product lookup never throws on one
- [ ] #6 Plant milks and butters (soyamelk, havremelk, kauramaito, plantesmør, oat milk) are not matched to dairy milk/butter
- [ ] #7 After the first-visit language sync, suggestions and ingredients refetch in that language
- [ ] #8 A unit label typed in any supported language is stored in English (kpl while Norwegian → pcs)
- [ ] #9 An oversized multipack count in product data yields no size instead of a 500
- [ ] #10 Tests cover the backend fixes and units; npm test + npm run typecheck pass
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Catalog.Search: match alias prefix or any word prefix; return all aliases (every language) so the form can resolve typed names. 2. Catalog.Match: dairy ingredients never match text with a plant marker (soy/oat/havre/kaura/mandel/plante...). 3. ItemFormPage: match by name or alias; apply opened-location in an effect when match arrives, unless the user picked a location. 4. ProfilePage: on language save error restore previous language. 5. App.syncProfile: invalidate all queries after a language write. 6. PATCH /profile: Enum.IsDefined check; lookup falls back to English. 7. storedUnit: reverse-map across all dictionaries. 8. PackageSize: TryParse bounded multipack count. 9. Tests + typecheck.
<!-- SECTION:PLAN:END -->
