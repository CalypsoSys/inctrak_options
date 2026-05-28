# Repository Guidelines

## Project Structure & Module Organization
The active .NET API project is `IncTrak.Api/`, which contains the stock-option management controllers, data
access, and EF models. The main authenticated frontend lives in `frontend/` as a Vue 3 + TypeScript + Vite SPA using
Vue Router, Pinia, Tailwind CSS, and PrimeVue. The public vesting and signup apps live in `frontend-vesting/` and
`frontend-signup/` as separate Vue + Vite frontends. Static marketing and documentation sites live in `inctrak.com/`
and `docs.inctrak.com/`; WordPress sites, including `blog.inctrak.com`, live in `../calypsosys-wordpress`. SQL reference scripts are under `inctrak.db/`. Docker deployment assets
live in `docker/inctrak/`, `scripts/inctrak/`, and `scripts/caddy/`; production operations are documented in
`docs/inctrak_production_runbook.md`.

## Build, Test, and Development Commands
Run commands from the repository root unless noted.

- `./build.sh` builds the API project and its test project from the repo root.
- `pnpm --dir frontend build` and `pnpm --dir frontend test` build and test the main SPA.
- `pnpm --dir frontend-vesting build` and `pnpm --dir frontend-vesting test` build and test the public
  vesting app.
- `pnpm --dir frontend-signup build` and `pnpm --dir frontend-signup test` build and test the public
  signup app.
- `dotnet build IncTrak.Api/IncTrak.Api.csproj` builds the API directly.
- `dotnet run --project IncTrak.Api/IncTrak.Api.csproj` starts the API locally.
- `dotnet test IncTrak.Api.Tests/IncTrak.Api.Tests.csproj` runs the API split tests.
- `node --test scripts/tests/inctrak-deploy.test.mjs` checks deployment docs and stack assets.

## Coding Style & Naming Conventions
Follow the existing C# style: 4-space indentation, braces on new lines, PascalCase for types and public members,
camelCase for locals and parameters. Keep controllers in `Controllers/`, EF models in `Models/` or `FeedbackModels/`,
and request/response helpers in `Data/dto/`. Preserve current file naming patterns such as `ScheduleController.cs`,
`ResetPassword.cs`, and `inctrakContext.cs`. No repo-wide formatter config is checked in, so match surrounding code
closely.

## Testing Guidelines
Run `./build.sh` and `dotnet test IncTrak.Api.Tests/IncTrak.Api.Tests.csproj` for behavior changes to the
API host. For SPA changes, run the affected frontend build and test scripts, then smoke-test the static site from a
local web server against the API. For deployment doc, Caddy, Docker, or rendered-config changes, run
`node --test scripts/tests/inctrak-deploy.test.mjs`. Add new tests in sibling `*.Tests` projects and name files after
the target type, for example `CorsOriginPolicyTests.cs`.

## Commit & Pull Request Guidelines
Recent commits use short, imperative subjects such as `Create README.md`, `misc cleanup`, and `docker`. Keep commit
titles brief, specific, and action-oriented. Pull requests should include a concise summary, note any config or schema
impact, list manual verification steps, and attach screenshots for UI changes in `frontend/` or the static sites.

## Security & Configuration Tips
Do not commit PII, secrets, passwords, or machine-specific settings. Follow security best practices when handling
sensitive data in code, including storage, transport, masking, and access control for PII, credentials, and secrets.
`.gitignore` already excludes `appsettings.json`, `ConnectionStrings.config`, and similar machine-specific files. Keep
environment-specific values out of source, and review SQL scripts in `inctrak.db/` carefully before applying them to
shared databases.

## Agent-Specific Instructions
In Codex CLI, any line prefixed with `>>>` must be treated as a request to add or update persistent repository
guidance in `AGENTS.md`, not as temporary session-only instruction. Preserve the user’s intent, keep additions concise,
and integrate them into the most relevant section instead of appending duplicate guidance.
Ask qualifying questions before proceeding with code changes so scope, constraints, and intent are clear.
Add unit tests for any new or modified code. If the repository lacks a suitable test project, create one as part of the
change unless the user explicitly says not to.
Use comments sparingly. Comment complex code and non-trivial methods, never remove existing comments, and avoid adding comments to trivial code.
Do not refactor code unrelated to the requested change. Avoid incidental cleanup such as rewriting loops, renaming
symbols, or reformatting untouched logic unless it is required to complete the task safely.
For post-change deliverables, always ask whether the user is ready for commit, PR, and ticket materials before producing
them. Only provide the requested combination after an affirmative response. Treat post-change deliverables as including
commit message format plus PR description and Jira ticket description templates.
