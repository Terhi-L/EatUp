---
id: TASK-13.1
title: Tag recipes with nutrition facts
status: In Progress
assignee:
  - '@claude'
created_date: '2026-11-05T13:01:00Z'
updated_date: '2026-11-19T15:02:00Z'
labels:
  - backend
  - domain
milestone: m-3
dependencies: []
parent_task_id: TASK-13
priority: medium
type: feature
ordinal: 13100
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The weekly health analysis needs to know per recipe: protein source, whether it has fish/omega-3, vegetable portions, and a fast-food/ultra-processed flag (PLAN.md phase 7). Recipes live in `backend/src/EatUp.Api/Seed/catalog.json`.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Every seed recipe has the nutrition tags, and a test fails if one is missing
- [x] #2 Tags are read into the domain model with no EF or HTTP dependencies
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Domain (Catalog.cs, no EF/HTTP): ProteinSource enum (None, Legumes, Egg, Dairy, Fish, Poultry, RedMeat, ProcessedMeat) and NutritionTags(Protein, Omega3, VegetablePortions, UltraProcessed), each field [JsonRequired] so partial tags fail to load; Recipe gets Nutrition; the Catalog constructor rejects a recipe without tags, like unknown keys.
2. Tests first: every seed recipe is tagged; a recipe without tags or with a missing field is rejected; spot checks (salmon = fish + omega-3, bolognese = red meat, potatoes don't count as vegetables).
3. Tag all 26 recipes in Seed/catalog.json. Definitions: main protein per serving (meat/fish over egg/dairy), vegetable portions = ~100 g of veg/fruit/berries per serving excluding potatoes (5-a-day), UltraProcessed = fast food or mostly ultra-processed (none of the seed recipes).
4. TestCatalog.Rec gets default tags. Log the tag definitions in docs/decisions.md.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
- Domain (Domain/Catalog.cs, Domain/Enums.cs, no EF/HTTP): ProteinSource enum and NutritionTags(Protein, Omega3, VegetablePortions, UltraProcessed) on Recipe. Every field is [JsonRequired], so a tag left out fails the load instead of silently becoming None/false/0. The Catalog constructor rejects recipes without tags, the same way it rejects unknown keys.
- All 26 recipes in Seed/catalog.json are tagged. Definitions are logged in docs/decisions.md (2026-11-19): one main protein, with meat or fish winning; vegetable portions as in 5-a-day, optional ingredients included, potatoes excluded; omega-3 for fish; ultra-processed false for every seed recipe.
- Evidence: NutritionTests. Every_seed_recipe_has_nutrition_tags, A_recipe_without_nutrition_tags_is_rejected and Nutrition_tags_missing_a_field_are_rejected (JsonException names vegetablePortions). Per-recipe cross-checks against the ingredients: Main_protein_comes_from_the_recipes_ingredients, Fish_dishes_are_an_omega3_source, Only_vegetables_and_fruit_count_as_vegetable_portions_not_potatoes.
- Mutation check: tagging bolognese as Dairy and salmon as non-omega-3 failed exactly those two cases.
- dotnet test 191 passed; Vitest 36 passed; typecheck passes. No API/DTO change, so no gen:api.
<!-- SECTION:NOTES:END -->
