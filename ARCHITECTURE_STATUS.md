# Architecture Status

This file tracks the current IncTrak architecture direction, what is already implemented, and what is still planned. It is intended to be a living status document that we update as decisions change and work moves from planned to partial to done.

Supporting detailed design documents:

- [docs/control-plane-schema.md](docs/control-plane-schema.md)

## Current Direction

IncTrak is moving toward a multi-tenant SaaS model with:

- a Vue + Vite frontend
- an API-only ASP.NET backend in `shared.inctrak.com/`
- Cloudflare edge routing for public and tenant hostnames
- a control-plane Postgres database for tenants, domains, memberships, and provisioning
- separate tenant Postgres databases created from a shared PostgreSQL template database
- managed authentication rather than homegrown password and OAuth flows

## Key Decisions

| Area | Decision | Status | Notes |
| --- | --- | --- | --- |
| Frontend hosting | Move toward Cloudflare Worker-hosted frontend and wildcard routing rather than Cloudflare Pages | Planned | Pages is not a good fit for `*.inctrak.com` tenant subdomains. |
| Backend hosting | Keep `shared.inctrak.com` API-only | Partial | API split and env-first config are already in place. |
| Tenant routing | Use `signup.inctrak.com`, `vesting.inctrak.com`, and `*.inctrak.com` | Planned | Worker should resolve hostnames and forward trusted tenant context. |
| Tenant site model | Use one tenant site per company rather than separate admin and participant hostnames | Planned | Admins and participants should both use `<tenant>.inctrak.com`, with role-based routing and authorization. |
| Tenant isolation | Use separate tenant databases rather than Postgres RLS initially | Planned | Safer fit for the current legacy/transition codebase. |
| Tenant database creation | Use PostgreSQL template cloning | Partial | `inctrak.db/inctrak.sql` is the bootstrap source for building `inctrak_template`, and runtime provisioning now expects to clone from a real template DB. |
| Authentication | Prefer Supabase Auth over continuing custom auth | Partial | Supabase bearer-token validation is now wired in the backend, with JWKS/public-key verification as the preferred path. |
| Public vesting tool | Keep quick vesting on a dedicated public hostname | Planned | Target hostname is `vesting.inctrak.com`. |

## Status Matrix

| Group / Feature / Section | Target State | Current Status | Notes |
| --- | --- | --- | --- |
| API-only backend | `shared.inctrak.com` serves API traffic only | Partial | Static web assets are now disabled at the project level; frontend/API split is in place. |
| Env-first config | Runtime config comes from env, not appsettings secrets | Partial | Gateway, logging, and local VS Code env rendering are already refactored. |
| VS Code local stack | One launcher starts frontend + backend together | Partial | Current local launcher works, and frontend debug now uses `127.0.0.1:5174` to avoid MMA collisions. |
| Cloudflare edge architecture | Worker serves SPA and routes wildcard subdomains | Not implemented | Current frontend still runs as a standard Vite SPA and deployment wiring has not been moved to Worker assets yet. |
| Signup hostname | `signup.inctrak.com` hosts tenant signup and company creation | Not implemented | Signup flow and company slug reservation do not exist yet. |
| Public vesting hostname | `vesting.inctrak.com` hosts quick vesting | Not implemented | Quick vesting exists in the SPA and API, but not yet as a dedicated public hostname/app mode. |
| Tenant hostname resolution | `*.inctrak.com` resolves tenants by subdomain | Not implemented | No tenant-control-plane or hostname resolver exists yet. |
| Tenant role routing | Same tenant hostname supports both admins and participants with different default routes | Partial | Request-scoped tenant/user context and a protected probe endpoint now exist, but real hostname-driven routing and app landing behavior are still pending. |
| Tenant authorization boundary | Participant/end-user accounts never gain admin access or admin API capability | Partial | Backend role-gated probe endpoints now enforce tenant admin versus participant access, but existing legacy app endpoints are not migrated yet. |
| Control-plane database | Stores tenants, slugs, memberships, provisioning jobs, and domains | Partial | `inctrak.db/control_plane.sql` now exists and the backend can resolve through a control-plane store, but production provisioning and broad runtime usage are still pending. |
| Tenant template database | `inctrak_template` used to clone new tenant databases | Partial | Bootstrap SQL exists in `inctrak.db/inctrak.sql`, and runtime provisioning now clones from a real template DB, but template refresh tooling is still manual. |
| Tenant provisioning pipeline | Create DB, seed tenant, create first admin, mark tenant ready | Partial | Current signup can clone a tenant DB from `inctrak_template` and seed the first admin, but async jobs and production-grade operational flow are still pending. |
| Managed auth | Supabase handles auth providers and sessions | Partial | Backend bearer-token validation is now in place, but the frontend login flow and real user provisioning are still pending. |
| Tenant-aware authorization | App maps authenticated users to tenants and roles from control-plane data | Partial | Trusted-header overrides remain, and the backend now supports Supabase-backed user resolution through the control-plane store. |
| Legacy auth retirement | Old password reset/activation/social flows are removed | Partial | Legacy mail sending has been neutralized, but old auth logic is still present. |
| Logging and audit | Access/error logging is file-based and production-friendly | Partial | File logging and access logging are in place; structured audit work is still open. |
| Public vesting UX | `frontend-vesting` is a focused public calculator with clear helper flows | Partial | Dedicated app exists, and schedule entry guidance is improving, but natural-language generation and broader polish are still in progress. |
| Public vesting schedule presets | Common schedules can be inserted from presets/examples | Planned | Examples exist in UI copy, but one-click preset application is still pending. |
| Public vesting prompt interpreter | Users can describe a schedule in plain English and get structured periods | Partial | The backend now uses a three-level `pattern -> parser -> AI` flow and converts everything through a richer `VestingDefinition` model before producing quick-builder periods, but broader language coverage and interactive clarification are still pending. |
| Vesting domain parser | Vesting text is parsed into a typed `VestingDefinition` with warnings, missing fields, and deterministic normalization | Partial | Rule extraction, built-in parser flow, and local-AI extraction now exist; next work is deeper schedule-kind coverage, better explicit tranche support, and broader phrase understanding. |
| Deterministic vesting generator | Final vesting events are generated in C# rather than trusted from AI output | Partial | A deterministic `VestingScheduleGenerator` now covers immediate, periodic, and standard cliff schedules plus segment-based rounding, but more advanced milestone and tranche cases still need expansion. |
| AI abstraction layer | Varying prompt interpreters can plug in behind one backend interface | Partial | `IVestingPromptInterpreter` plus ordered provider adapters isolate the controller from concrete backends, and AI providers now return `VestingDefinition` data instead of raw schedule periods. |
| Embedded local AI via LlamaSharp | Backend can run a GGUF model in-process for offline vesting interpretation | Partial | Preferred self-hosted path; it now feeds the V2 vesting-definition pipeline, while runtime tuning, model guidance, and operational packaging are still open. |
| Local AI via Microsoft.Extensions.AI | Backend can swap in `IChatClient`-style local or hosted model integrations later | Planned | Preferred abstraction for future AI-backed schedule interpretation because it keeps provider choice open. |
| Local AI via Ollama | Local model inference can be used for schedule interpretation during development or self-hosted installs | Planned | Good fit for quick experimentation and narrow JSON output if we later want a local non-Microsoft runtime. |
| Local AI via Foundry Local | Microsoft-native local model runtime can power schedule interpretation on supported devices | Planned | Attractive for a future Windows-first local AI path, but not needed for the phase-1 rules-based release. |
| Hosted external AI adapter | Backend can swap to a hosted model provider without changing controller/UI contracts | Planned | The provider boundary is in place, but local embedded inference remains the preferred path and HTTP-based AI is now treated as an optional adapter rather than the primary architecture. |
| AI follow-up questions | Interpreter can ask for missing or ambiguous schedule details instead of guessing | Planned | Current flow returns best-effort periods or an error; interactive clarification is still not implemented. |
| GGUF model operations | Local model download, refresh, sizing, and machine guidance are documented and repeatable | Not implemented | We still need concrete guidance for model files, memory footprint, CPU/GPU expectations, and update workflow. |

## Authentication Recommendation

Current recommendation:

- use Supabase Auth for:
  - email + password
  - magic link or email code
  - Google
  - Microsoft
  - Apple later if needed
- keep IncTrak responsible for:
  - tenant membership
  - organization roles
  - company slug/domain ownership
  - tenant provisioning

Application-shape recommendation:

- customer admins and customer end users should both access the same tenant hostname
- the application should choose landing routes based on role after authentication
- admin-only capabilities must be enforced server-side, even if the frontend hides those routes and controls

Why this is the current recommendation:

- it aligns better than Firebase with the Postgres-first direction
- it avoids continuing the current homegrown auth complexity
- it leaves room for future tenant-aware authorization without coupling identity to tenant databases

Open follow-up questions:

- whether Apple should be launch scope or phase 2
- whether Supabase-hosted login pages or custom UI should be the first integration
- how strongly we want to support multi-org membership for one user in v1

## Tenant Database Recommendation

Current recommendation:

- one control-plane database
- one tenant database per company
- use a PostgreSQL template database named something like `inctrak_template`
- create tenant databases with `CREATE DATABASE ... TEMPLATE inctrak_template`

Current assets:

- [inctrak.db/inctrak.sql](inctrak.db/inctrak.sql)
  - canonical bootstrap source for tenant databases and any `inctrak_template` clone source

Still needed:

- a helper script to create or refresh `inctrak_template` from `inctrak.db/inctrak.sql`
- a provisioning worker/job runner
- a template versioning strategy tied to migrations

## Localhost Port Strategy

Recommended local ports:

| Purpose | Host | Port | Notes |
| --- | --- | --- | --- |
| Main frontend SPA | `127.0.0.1` | `5174` | Already in use for this repo to avoid conflict with MMA on `5173`. |
| Local backend HTTP | `localhost` | `5000` | Already in use by `shared.inctrak.com`. |
| Local backend HTTPS | `localhost` | `5001` | Already in use by `shared.inctrak.com`. |
| Signup frontend mode | `127.0.0.1` | `5175` | Suggested future port when signup becomes its own app mode or dev entrypoint. |
| Public vesting frontend mode | `127.0.0.1` | `5176` | Suggested future port for dedicated quick-vesting dev flow. |
| Optional Worker local dev | `127.0.0.1` | `8788` | Suggested future port if a Cloudflare Worker shell is added locally. |
| Optional Supabase local stack | default Supabase local ports | external | Keep Supabase on its standard local toolchain ports rather than reassigning them here. |

Recommended local hostname mapping later, if we want realistic subdomain testing:

- `signup.inctrak.localhost`
- `vesting.inctrak.localhost`
- `calypsosys.inctrak.localhost`

But for now, distinct ports are the simpler first step.

## Practical Build Order

Recommended next phases:

1. Introduce Supabase-backed auth strategy and control-plane user model.
2. Design the control-plane schema for tenants, slugs, domains, memberships, roles, and provisioning jobs.
3. Define the admin versus participant authorization model and enforce it in backend APIs.
4. Add backend tenant context resolution and tenant-aware connection factories.
5. Add a helper to create `inctrak_template` from `inctrak.db/inctrak.sql`.
6. Build the signup flow and tenant slug reservation path.
7. Split quick vesting into a dedicated public frontend mode for `vesting.inctrak.com`.
8. Introduce Worker-based hostname routing for `signup`, `vesting`, and `*.inctrak.com`.

## Implemented Foundations

These pieces already support the future architecture:

- API/frontend split exists
- Vite frontend exists in `frontend/`
- API host is env-first and API-only
- VS Code local launcher exists for frontend + backend
- file-based access and error logging exist
- API rate limiting exists
- control-plane bootstrap SQL now exists for the shared tenant/identity metadata database
- request-scoped control-plane tenant/user context scaffolding now exists in the backend
- control-plane store-backed tenant, user, and membership resolution now exists in the backend
- Supabase bearer-token auth now exists in the backend, with JWKS/public-key verification as the preferred path
- tenant admin versus participant enforcement now has a protected backend probe path with tests
- tenant bootstrap SQL now exists in `inctrak.db/inctrak.sql` for tenant DB cloning

## Open Risks

- legacy auth code is still present and can become a drag on the new auth model if we do not isolate replacement work cleanly
- tenant provisioning will affect connection management, migrations, and operational tooling, so it should be introduced deliberately
- Cloudflare Pages is not the right long-term fit if wildcard tenant subdomains are a real product requirement
- local dev complexity can grow quickly once signup, vesting, tenant, Worker, backend, and Supabase flows all coexist
- role separation must be enforced in backend authorization from the start so participant users cannot accidentally reach admin APIs through URL guessing or direct requests

## Update Conventions

When updating this file:

- move status as work progresses: `Not implemented` -> `Planned` -> `Partial` -> `Done`
- keep decisions explicit when we reject an alternative
- add new work items rather than hiding them in prose
- prefer short notes over long design essays here; deeper designs can live in `docs/` later
