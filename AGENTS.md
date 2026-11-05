# EatUp

Food-waste PWA: track what's in the fridge, get reminded before it expires, get recipes that use it up. React + TypeScript + Vite frontend (`web/`), ASP.NET Core .NET 10 minimal API + EF Core SQLite backend (`backend/`). Single local user, no login.

## Setup and verification

- Setup: `npm install`, then `npm run dev` (API on :5080, web on :5173 with `/api` proxied). Full guide: `GETTING_STARTED.md`.
- Verify every change with `npm test` (xUnit + Vitest) and `npm run typecheck`. Done means both pass.
- Reset local data: stop the app and delete `backend/data/`.

## Picking up work

- Next task: the first line of `backlog task list --status "To Do" --ready --sort ordinal --plain`. Ordinal is the agreed build order; `--ready` hides tasks whose dependencies aren't Done.
- Skip a task labelled `needs-decision` until the product owner has confirmed the approach; ask them, or take the next one.
- An In Progress task assigned to you comes first; one waiting on a human (e.g. translation review) is not yours to finish.

## Rules

### 1. Questions get answers, not changes
**Where:** any user message phrased as a question or idea ("can we…", "what about…", "should…").
**Rule:** answer it, weigh it critically (trade-offs, risks, a better alternative if there is one), then wait. Edit files only after the user confirms.
**Why:** the user often wants to discuss before deciding; unrequested changes have to be reviewed and undone.

### 2. Terse replies
**Where:** every reply to the user.
**Rule:** max 3 sentences or 5 bullets. Answer or result first. Skip what worked as expected; mention only failures, surprises and what the user must do. No headings, no file lists. The user asks when they want an explanation.
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

## Project notes

- Read `PLAN.md` for the roadmap and `docs/decisions.md` for how the build deviates from it. Log new deviations there.
- Domain rules (`backend/src/EatUp.Api/Domain/`) stay free of EF/HTTP and get unit tests first.
- Dates: `DateOnly` for expiry; compute "today" in the user's time zone via `Expiry.LocalToday`.
- When changing an endpoint DTO in `Endpoints.cs`, update `web/src/api.ts` to match. API writes need the `X-EatUp` header (`api.ts` sends it).
- No migrations yet: after changing the EF model, delete `backend/data/`.
- UI text goes in all three dictionaries in `web/src/i18n/` (`en.ts` defines the keys); components read it through `web/src/strings.ts`.
- On Windows, paths are case-insensitive: `backend/src/EatUp.Api/Data/` (source) is not `backend/data/` (runtime files); don't create a lowercase `data` folder inside the API project.
