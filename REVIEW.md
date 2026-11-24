# Review guidance for EatUp

EatUp is a food-waste PWA built in roadmap phases (`PLAN.md`); `docs/decisions.md` records where the build deviates, and `AGENTS.md` holds the coding rules.

## Judge a PR by its own phase

- Today EatUp has one implicit local user and no login. Accounts (TASK-16) and households shared by several people (TASK-17, planned in TASK-19) come in later phases.
- Until a PR builds accounts or households, don't raise findings that only matter with several users or devices: missing authentication or authorization on reads, per-user or per-household data scoping, or races between two tabs or devices that don't lose or corrupt data. Raise them on the PR that adds accounts or households.
- Always raise security and data-safety defects in what the PR builds, for example a write endpoint without the `X-EatUp` header check, or a migration that loses data.

## Catch untranslated UI text

- Every user-facing text exists in all three dictionaries in `web/src/i18n/` (`en.ts`, `nb.ts`, `fi.ts`) and components read it through `web/src/strings.ts`.
- Report UI text that is missing from a dictionary, hard-coded in a component, or left in English in `nb.ts` or `fi.ts`.
