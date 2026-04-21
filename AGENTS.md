# Repository Guidelines

## Project Structure & Module Organization
The active .NET projects are `goals.inctrak.com/`, `shared.inctrak.com/`, and `ModelDriver/`. `goals.inctrak.com/` is the main ASP.NET Core web app (`Controllers/`, `Data/`, `Domain/`, `Models/`, `FeedbackModels/`, `wwwroot/`). `shared.inctrak.com/` contains the stock-option management app and shared static assets. `ModelDriver/` is a small console project for model and database work. Static marketing and documentation sites live in `inctrak.com/`, `docs.inctrak.com/`, and `blog.inctrak.com/` as plain static folders. SQL reference scripts are under `inctrak.db/`.

## Build, Test, and Development Commands
Run commands from the repository root unless noted.

- `./build.sh` builds all active .NET projects from the repo root.
- `dotnet build goals.inctrak.com/goals.inctrak.com.csproj` builds the goal-setting app directly.
- `dotnet build shared.inctrak.com/shared.inctrak.com.csproj` builds the stock-option app directly.
- `dotnet build ModelDriver/ModelDriver.csproj` builds the console utility directly.
- `dotnet run --project goals.inctrak.com/goals.inctrak.com.csproj` starts the goal-setting app locally on the URLs in `Properties/launchSettings.json`.
- `dotnet run --project ModelDriver/ModelDriver.csproj` runs the console utility.
- `dotnet test` currently finds no test projects; add a dedicated test project before relying on automated coverage.

## Coding Style & Naming Conventions
Follow the existing C# style: 4-space indentation, braces on new lines, PascalCase for types and public members, camelCase for locals and parameters. Keep controllers in `Controllers/`, EF models in `Models/` or `FeedbackModels/`, and request/response helpers in `Data/dto/`. Preserve current file naming patterns such as `GoalController.cs`, `ResetPassword.cs`, and `inctrak_goalsContext.cs`. No repo-wide formatter config is checked in, so match surrounding code closely.

## Testing Guidelines
There is no committed test suite yet. For behavior changes, at minimum run `./build.sh` and smoke-test the affected endpoints or pages locally. When adding tests, prefer xUnit in a sibling `*.Tests` project and name files after the target class, for example `GoalControllerTests.cs`.

## Commit & Pull Request Guidelines
Recent commits use short, imperative subjects such as `Create README.md`, `misc cleanup`, and `docker`. Keep commit titles brief, specific, and action-oriented. Pull requests should include a concise summary, note any config or schema impact, list manual verification steps, and attach screenshots for UI changes in `wwwroot/` or the static sites.

## Security & Configuration Tips
Do not commit PII, secrets, passwords, or machine-specific settings. Follow security best practices when handling sensitive data in code, including storage, transport, masking, and access control for PII, credentials, and secrets. `.gitignore` already excludes `appsettings.json`, `ConnectionStrings.config`, and similar machine-specific files. Keep environment-specific values out of source, and review SQL scripts in `inctrak.db/` carefully before applying them to shared databases.

## Agent-Specific Instructions
In Codex CLI, any line prefixed with `>>>` must be treated as a request to add or update persistent repository guidance in `AGENTS.md`, not as temporary session-only instruction. Preserve the user’s intent, keep additions concise, and integrate them into the most relevant section instead of appending duplicate guidance.
Ask qualifying questions before proceeding with code changes so scope, constraints, and intent are clear.
Add unit tests for any new or modified code. If the repository lacks a suitable test project, create one as part of the change unless the user explicitly says not to.
Use comments sparingly. Comment complex code and non-trivial methods, never remove existing comments, and avoid adding comments to trivial code.
Do not refactor code unrelated to the requested change. Avoid incidental cleanup such as rewriting loops, renaming symbols, or reformatting untouched logic unless it is required to complete the task safely.
For post-change deliverables, always ask whether the user is ready for commit, PR, and ticket materials before producing them. Only provide the requested combination after an affirmative response. Treat post-change deliverables as including commit message format plus PR description and Jira ticket description templates.
