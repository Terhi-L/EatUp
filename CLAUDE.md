# EatUp: notes for coding agents

Food-waste PWA: React + TypeScript + Vite frontend (`web/`), ASP.NET Core .NET 10 minimal API + EF Core SQLite backend (`backend/`).

- Run: `npm run dev` (API on :5080, web on :5173 with `/api` proxied). Test: `npm test`. Type-check: `npm run typecheck`.
- Read `PLAN.md` for the roadmap, and `docs/decisions.md` for how the built MVP deviates from it. Log new deviations there.
- **Zero cost, local first**: nothing paid, nothing that needs a card. Ask before adding any external service.
- Domain rules (`backend/src/EatUp.Api/Domain/`) stay free of EF/HTTP and get unit tests first.
- Dates: `DateOnly` for expiry; compute "today" in the user's time zone via `Expiry.LocalToday`; use the injected `TimeProvider`, never `DateTime.Now`.
- When changing an endpoint DTO in `Endpoints.cs`, update `web/src/api.ts` to match.
- No migrations yet: after changing the EF model, delete `backend/data/`.
- UI text goes in `web/src/strings.ts`.
- On Windows, paths are case-insensitive: `backend/src/EatUp.Api/Data/` (source) is not the same as `backend/data/` (runtime files), so don't create a lowercase `data` folder inside the API project.
