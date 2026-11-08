# EatUp

Food-waste PWA: track what's in the fridge, get reminded before it expires, get recipes that use it up. React + TypeScript + Vite frontend (`web/`), ASP.NET Core .NET 10 minimal API + EF Core SQLite backend (`backend/`). Single local user, no login.

## Setup and verification

- Setup: `npm install`, then `npm run dev` (API on :5080, web on :5173 with `/api` proxied). Full guide: `GETTING_STARTED.md`.
- Verify every change with `npm test` (xUnit + Vitest) and `npm run typecheck`. Done means both pass.
- Reset local data: stop the app and delete `backend/data/eatup.db`.

## Picking up work

- Next task: the first line of `backlog task list --status "To Do" --ready --sort ordinal --plain`. Ordinal is the agreed build order; `--ready` hides tasks whose dependencies aren't Done.
- Skip a task labelled `needs-decision` until the product owner has confirmed the approach; ask them, or take the next one.
- An In Progress task assigned to you comes first; one waiting on a human (e.g. translation review) is not yours to finish.
- Backlog is the source of truth for progress; don't track status in `PLAN.md` or other docs. Set a task to Done (with its final summary) in the task's own PR, so merging it is what finishes the task; never open a PR just to change a status.

## Rules

### 1. Questions get answers, not changes
**Where:** any user message phrased as a question or idea ("can we…", "what about…", "should…").
**Rule:** answer it, weigh it critically (trade-offs, risks, a better alternative if there is one), then wait. Edit files only after the user confirms.
**Why:** the user often wants to discuss before deciding; unrequested changes have to be reviewed and undone.

### 2. Terse replies
**Where:** every reply to the user.
**Rule:** one sentence; add one more short sentence only when something important (a failure, risk or decision the user must make) would otherwise be lost. Answer or result first. Skip what worked as expected; mention only failures, surprises and what the user must do. No headings, no file lists. The user asks when they want an explanation.
**Why:** long replies bury the one line the user needs.

### 3. Self-explaining code
**Where:** all C# and TypeScript.
**Rule:** descriptive names carry the meaning; comments are rare, one line, and say *why*, never *what*. DRY: extract repeated logic once. Inject dependencies (constructor or parameter DI, `TimeProvider` instead of `DateTime.Now`); no hard-wired `new` of services.
**Why:** a comment that restates its method name (`// Gets the food item count` above `GetFoodItemCount`) is noise that goes stale; DI keeps code testable.

### 4. Short methods with early returns
**Where:** all C# and TypeScript functions, including endpoint handlers and React components.
**Rule:** guard clauses and early returns first, happy path last and unindented. When a method covers more than one topic, split each topic into its own well-named method and let the original read as a list of calls.
**Why:** one topic per method reads top to bottom without holding a stack of nested logic in your head.

### 5. Zero cost, local first
**Where:** dependencies, services, hosting.
**Rule:** free and local only; ask before adding any external service.
**Why:** the project must run with nothing paid and no card required (`PLAN.md` § 0).

### 6. Review against the roadmap
**Where:** every PR review, by Qodo, agents or people.
**Rule:** judge a PR by its own task and the current phase in `PLAN.md` and `docs/decisions.md`. A feature the roadmap schedules for later (login, OpenAPI-generated types) is a finding only on the PR that implements it. Security and data-safety defects in what the PR builds, such as a write without the `X-EatUp` header check, are always findings. Rule 5 applies to every PR.
**Why:** the MVP postpones roadmap work on purpose; findings that ask for it early are noise.

## Project notes

- Read `PLAN.md` for the roadmap and `docs/decisions.md` for how the build deviates from it. Log new deviations there.
- Domain rules (`backend/src/EatUp.Api/Domain/`) stay free of EF/HTTP and get unit tests first.
- Dates: `DateOnly` for expiry; compute "today" in the user's time zone via `Expiry.LocalToday`.
- After changing an endpoint or DTO in `Endpoints.cs`, run `npm run gen:api` and commit the regenerated `web/openapi.json` and `web/src/api-schema.d.ts`; never edit those by hand. Give an endpoint returning `IResult` a `.Produces<T>()` so its response type is generated. API writes need the `X-EatUp` header (`api.ts` sends it).
- After changing the EF model, add a migration (`dotnet ef migrations add <Name> --project backend/src/EatUp.Api --output-dir Data/Migrations`); never make users delete their database. The `Migrations_match_the_model` test fails until you do.
- UI text goes in all three dictionaries in `web/src/i18n/` (`en.ts` defines the keys); components read it through `web/src/strings.ts`.
- On Windows, paths are case-insensitive: `backend/src/EatUp.Api/Data/` (source) is not `backend/data/` (runtime files); don't create a lowercase `data` folder inside the API project.
