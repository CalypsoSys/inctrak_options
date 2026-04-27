# Architecture Status

This file tracks the current IncTrak architecture direction, what is already implemented, and what is still planned. It is intended to be a living status document that we update as decisions change and work moves from planned to partial to done.

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
| Tenant isolation | Use separate tenant databases rather than Postgres RLS initially | Planned | Safer fit for the current legacy/transition codebase. |
| Tenant database creation | Use PostgreSQL template cloning | Partial | `inctrak.db/template-bootstrap.sql` now exists as the bootstrap source for `inctrak_template`. |
| Authentication | Prefer Supabase Auth over continuing custom auth | Planned | Supabase currently looks like the best fit among the low-cost managed options considered so far. |
| Public vesting tool | Keep quick vesting on a dedicated public hostname | Planned | Target hostname is `vesting.inctrak.com`. |

## Status Matrix

| Area | Target State | Current Status | Notes |
| --- | --- | --- | --- |
| API-only backend | `shared.inctrak.com` serves API traffic only | Partial | Static web assets are now disabled at the project level; frontend/API split is in place. |
| Env-first config | Runtime config comes from env, not appsettings secrets | Partial | Gateway, logging, and local VS Code env rendering are already refactored. |
| VS Code local stack | One launcher starts frontend + backend together | Partial | Current local launcher works, and frontend debug now uses `127.0.0.1:5174` to avoid MMA collisions. |
| Cloudflare edge architecture | Worker serves SPA and routes wildcard subdomains | Not implemented | Current frontend still runs as a standard Vite SPA and deployment wiring has not been moved to Worker assets yet. |
| Signup hostname | `signup.inctrak.com` hosts tenant signup and company creation | Not implemented | Signup flow and company slug reservation do not exist yet. |
| Public vesting hostname | `vesting.inctrak.com` hosts quick vesting | Not implemented | Quick vesting exists in the SPA and API, but not yet as a dedicated public hostname/app mode. |
| Tenant hostname resolution | `*.inctrak.com` resolves tenants by subdomain | Not implemented | No tenant-control-plane or hostname resolver exists yet. |
| Control-plane database | Stores tenants, slugs, memberships, provisioning jobs, and domains | Not implemented | Schema still needs to be designed and introduced. |
| Tenant template database | `inctrak_template` used to clone new tenant databases | Partial | Bootstrap SQL exists; template creation and provisioning automation do not yet exist. |
| Tenant provisioning pipeline | Create DB, seed tenant, create first admin, mark tenant ready | Not implemented | Needs async job flow and operational state tracking. |
| Managed auth | Supabase handles auth providers and sessions | Not implemented | Current app still contains legacy auth tables and flows. |
| Tenant-aware authorization | App maps authenticated users to tenants and roles from control-plane data | Not implemented | This should replace assumptions built into the older group/user model over time. |
| Legacy auth retirement | Old password reset/activation/social flows are removed | Partial | Legacy mail sending has been neutralized, but old auth logic is still present. |
| Logging and audit | Access/error logging is file-based and production-friendly | Partial | File logging and access logging are in place; structured audit work is still open. |

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

- [inctrak.db/template-bootstrap.sql](inctrak.db/template-bootstrap.sql)
  - bootstrap source for creating the template database
- [inctrak.db/inctrak.sql](inctrak.db/inctrak.sql)
  - legacy schema reference

Still needed:

- a helper script to create or refresh `inctrak_template`
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
2. Design the control-plane schema for tenants, slugs, domains, memberships, and provisioning jobs.
3. Add backend tenant context resolution and tenant-aware connection factories.
4. Add a helper to create `inctrak_template` from `inctrak.db/template-bootstrap.sql`.
5. Build the signup flow and tenant slug reservation path.
6. Split quick vesting into a dedicated public frontend mode for `vesting.inctrak.com`.
7. Introduce Worker-based hostname routing for `signup`, `vesting`, and `*.inctrak.com`.

## Implemented Foundations

These pieces already support the future architecture:

- API/frontend split exists
- Vite frontend exists in `frontend/`
- API host is env-first and API-only
- VS Code local launcher exists for frontend + backend
- file-based access and error logging exist
- API rate limiting exists
- template bootstrap SQL now exists for tenant DB cloning

## Open Risks

- legacy auth code is still present and can become a drag on the new auth model if we do not isolate replacement work cleanly
- tenant provisioning will affect connection management, migrations, and operational tooling, so it should be introduced deliberately
- Cloudflare Pages is not the right long-term fit if wildcard tenant subdomains are a real product requirement
- local dev complexity can grow quickly once signup, vesting, tenant, Worker, backend, and Supabase flows all coexist

## Update Conventions

When updating this file:

- move status as work progresses: `Not implemented` -> `Planned` -> `Partial` -> `Done`
- keep decisions explicit when we reject an alternative
- add new work items rather than hiding them in prose
- prefer short notes over long design essays here; deeper designs can live in `docs/` later
