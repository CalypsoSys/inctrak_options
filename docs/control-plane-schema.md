# IncTrak Control-Plane Schema

This document defines the first concrete design for the IncTrak control-plane data model and request flows.

The control-plane is responsible for:

- tenants
- subdomains and domains
- user identities linked to Supabase Auth
- tenant memberships and roles
- signup state
- tenant provisioning jobs
- mapping requests to the correct tenant database

It is not responsible for storing tenant business data such as grants, schedules, stock classes, or participant vesting details. Those remain in per-tenant databases cloned from `inctrak_template`.

Current repo status:

- the initial bootstrap SQL now exists at [inctrak.db/control_plane.sql](../inctrak.db/control_plane.sql)
- backend request-scoped tenant/user context scaffolding now exists
- backend probe endpoints now prove tenant admin versus participant authorization behavior
- the backend now supports a real control-plane store path for tenant, user, and membership lookup
- Supabase integration still does not exist yet

## Goals

- support `signup.inctrak.com`, `vesting.inctrak.com`, and `*.inctrak.com`
- support one tenant hostname per company
- support both admins and participants on the same tenant hostname
- ensure participant users never gain admin capabilities
- support Supabase Auth without duplicating password and OAuth logic locally
- keep tenant provisioning asynchronous and observable

## High-Level Model

There are three core concepts:

1. Identity
- a human user authenticated through Supabase

2. Tenant
- a company/account with its own hostname and its own tenant database

3. Membership
- the relationship between a user and a tenant, including role

The control-plane database is the source of truth for these relationships.

Important boundary:

- `cp_users` is global across the platform
- tenant access is granted only through `cp_memberships`
- a user existing in `cp_users` alone does not grant access to any tenant

## When To Create The Database

Recommended timing:

- create a local development control-plane database now if you want to begin schema validation and resolver integration work
- do not create a long-lived production control-plane database yet unless you are ready to commit to the first naming and configuration shape

Why:

- the bootstrap schema is stable enough for local development
- runtime configuration and real DB-backed resolver services are now wired
- creating a production database too early tends to lock in names and assumptions before the app actually uses them

Practical recommendation:

- yes for local development
- not yet for production or shared environments

For local seeding convenience, the repo now includes:

- [inctrak.db/control_plane.seed.example.sql](../inctrak.db/control_plane.seed.example.sql)

That example seed file includes idempotent SQL for:

- one `cp_users` row
- one `cp_tenants` row
- one `cp_tenant_domains` row
- one `cp_memberships` row

Recommended local hostname pattern:

- `*.inctrak.localhost`

Current example:

- `calypsosys.inctrak.localhost`

## Roles

Recommended first role set:

- `tenant_admin`
  - full tenant administration
  - can manage participants, plans, grants, schedules, and tenant membership
- `tenant_participant`
  - end user / optionee view only
  - can only access their own participant-facing data
- `platform_operator`
  - reserved for internal IncTrak operations later
  - not part of normal tenant membership UI

Important rule:

- tenant participants must never be able to call admin APIs successfully
- backend authorization is the enforcement boundary
- frontend route hiding is convenience only, not protection

## Control-Plane Tables

### `cp_users`

Represents a user authenticated through Supabase.

Suggested columns:

- `user_pk uuid primary key`
- `supabase_user_id uuid not null unique`
- `email_address varchar(256) not null`
- `display_name varchar(256) null`
- `is_platform_operator boolean not null default false`
- `created timestamp with time zone not null default now()`
- `updated timestamp with time zone not null default now()`
- `last_login timestamp with time zone null`

Notes:

- `supabase_user_id` is the durable external identity key
- email should not be the primary key because providers and user state can change

### `cp_tenants`

Represents a tenant/company.

Suggested columns:

- `tenant_pk uuid primary key`
- `company_name varchar(256) not null`
- `tenant_slug varchar(63) not null unique`
- `status varchar(30) not null`
- `primary_domain varchar(255) not null`
- `tenant_db_name varchar(128) null`
- `template_version varchar(100) null`
- `created timestamp with time zone not null default now()`
- `updated timestamp with time zone not null default now()`
- `activated timestamp with time zone null`

Suggested status values:

- `PendingSignup`
- `Provisioning`
- `Active`
- `Suspended`
- `Failed`
- `Archived`

Notes:

- `tenant_slug` drives `<slug>.inctrak.com`
- `tenant_db_name` is null until provisioning succeeds

### `cp_tenant_domains`

Maps hostnames to tenants.

Suggested columns:

- `tenant_domain_pk uuid primary key`
- `tenant_fk uuid not null references cp_tenants`
- `host_name varchar(255) not null unique`
- `is_primary boolean not null default false`
- `verification_status varchar(30) not null`
- `created timestamp with time zone not null default now()`
- `updated timestamp with time zone not null default now()`

Initial use:

- store the generated default hostname like `calypsosys.inctrak.com`

Future use:

- support custom domains if needed

### `cp_memberships`

Maps users to tenants and roles.

Suggested columns:

- `membership_pk uuid primary key`
- `tenant_fk uuid not null references cp_tenants`
- `user_fk uuid not null references cp_users`
- `role_code varchar(50) not null`
- `status varchar(30) not null`
- `participant_external_key varchar(128) null`
- `created timestamp with time zone not null default now()`
- `updated timestamp with time zone not null default now()`
- `invited_by_user_fk uuid null references cp_users`
- `accepted timestamp with time zone null`

Suggested status values:

- `PendingInvite`
- `Active`
- `Suspended`
- `Revoked`

Notes:

- `participant_external_key` can later help map a participant-facing user to the corresponding tenant-database participant record
- unique constraint should exist on `(tenant_fk, user_fk)`

### `cp_signup_requests`

Tracks self-serve signup attempts and onboarding state.

Suggested columns:

- `signup_request_pk uuid primary key`
- `email_address varchar(256) not null`
- `company_name varchar(256) not null`
- `requested_slug varchar(63) not null`
- `normalized_slug varchar(63) not null`
- `status varchar(30) not null`
- `requested_by_user_fk uuid null references cp_users`
- `tenant_fk uuid null references cp_tenants`
- `created timestamp with time zone not null default now()`
- `updated timestamp with time zone not null default now()`
- `notes text null`

Suggested status values:

- `Started`
- `Reserved`
- `Provisioning`
- `Completed`
- `Rejected`
- `Failed`

### `cp_provisioning_jobs`

Tracks async tenant provisioning.

Suggested columns:

- `provisioning_job_pk uuid primary key`
- `tenant_fk uuid not null references cp_tenants`
- `job_type varchar(50) not null`
- `status varchar(30) not null`
- `attempt_count int not null default 0`
- `requested_by_user_fk uuid null references cp_users`
- `payload_json text null`
- `result_json text null`
- `started timestamp with time zone null`
- `finished timestamp with time zone null`
- `created timestamp with time zone not null default now()`
- `updated timestamp with time zone not null default now()`

Suggested job types:

- `CreateTenantDatabase`
- `SeedTenantAdmin`
- `VerifyTenantReady`

Suggested status values:

- `Pending`
- `Running`
- `Succeeded`
- `Failed`

### `cp_reserved_slugs`

Keeps unsafe or restricted slugs out of circulation.

Suggested columns:

- `reserved_slug_pk uuid primary key`
- `slug_value varchar(63) not null unique`
- `reason varchar(256) not null`
- `created timestamp with time zone not null default now()`

Initial reserved values should include:

- `www`
- `api`
- `shared`
- `signup`
- `vesting`
- `docs`
- `blog`
- `admin`
- `app`
- `support`

## Request and Flow Design

### 1. Self-Serve Signup Flow

Host:

- `signup.inctrak.com`

Flow:

1. user enters company name, email, and desired slug
2. frontend validates slug format
3. backend checks:
- slug is normalized
- slug is not reserved
- slug is not already in use
4. backend creates:
- `cp_users` row if needed after Supabase identity exists
- `cp_signup_requests`
- `cp_tenants` with `PendingSignup`
- `cp_tenant_domains`
- `cp_memberships` for first `tenant_admin`
- `cp_provisioning_jobs`
5. async provisioner creates tenant DB from `inctrak_template`
6. tenant moves to `Active`
7. user is redirected to `https://<slug>.inctrak.com`

### 2. Tenant Hostname Resolution

Host:

- `<slug>.inctrak.com`

Flow:

1. Worker or gateway extracts hostname
2. control-plane lookup finds `cp_tenant_domains.host_name`
3. system resolves:
- `tenant_pk`
- tenant status
- tenant DB name
4. trusted tenant headers are forwarded to `shared.inctrak.com`
5. backend resolves tenant context from trusted headers only

Important:

- browser must never be trusted to declare its own tenant

### 3. Authenticated User Resolution

Flow:

1. user signs in with Supabase
2. backend validates Supabase JWT or session
3. backend loads `cp_users` by `supabase_user_id`
4. backend loads membership for the resolved tenant
5. backend builds request `TenantContext` and `UserContext`

Context should include:

- `tenant_pk`
- `tenant_slug`
- `tenant_db_name`
- `user_pk`
- `role_code`
- `is_platform_operator`

### 4. Authorization Flow

Admin routes:

- require `tenant_admin`

Participant routes:

- require authenticated membership in the tenant
- may require `tenant_participant` or `tenant_admin`, depending on endpoint

Rules:

- `tenant_participant` cannot access admin endpoints
- `tenant_admin` can access participant-facing endpoints if useful
- internal operator capabilities should be introduced separately later

## Tenant Database Boundary

The control-plane does not replace tenant databases.

Control-plane owns:

- identity linkage
- memberships
- tenant metadata
- provisioning state

Tenant DB owns:

- groups, participants, plans, grants, schedules, periods, stock classes, and related business data

The long-term migration path should reduce dependence on legacy `GROUPS` as the top-level auth boundary and move that responsibility into control-plane tenant context.

## First Implementation Targets

Recommended first implementation slice:

1. create control-plane schema migration or bootstrap SQL for:
- `cp_users`
- `cp_tenants`
- `cp_tenant_domains`
- `cp_memberships`
- `cp_provisioning_jobs`
- `cp_reserved_slugs`

2. create backend domain types for:
- `TenantContext`
- `UserContext`
- `MembershipRole`

3. create lookup services for:
- `ITenantResolver`
- `IUserResolver`
- `IMembershipResolver`

4. add a simple protected endpoint that proves:
- tenant resolution works
- user resolution works
- admin versus participant enforcement works

This gives us the minimum safe foundation before we change the full app auth flow.

## Recommended Config Shape

These settings are now the intended config shape for the control-plane resolver path.

Suggested future `AppSettings` keys:

- `ControlPlaneConnection`
  - Postgres connection string for the shared control-plane database
- `TenantTemplateDatabaseName`
  - expected template DB name, for example `inctrak_template`
- `TenantDatabasePrefix`
  - prefix for provisioned tenant DBs, for example `inctrak_`
- `SupabaseUrl`
  - Supabase project URL
- `SupabaseAnonKey`
  - frontend/public Supabase key for browser auth flows
- `SupabaseJwtSecret`
  - backend JWT validation secret or equivalent verifier configuration

Suggested future YAML shape:

```yaml
AppSettings:
  ControlPlaneConnection: Host=localhost;Port=5432;Database=inctrak_control;Username=postgres;Password=${INCTRAK_CONTROL_DB_PASSWORD}
  TenantTemplateDatabaseName: inctrak_template
  TenantDatabasePrefix: inctrak_
  SupabaseUrl: ${INCTRAK_SUPABASE_URL}
  SupabaseAnonKey: ${INCTRAK_SUPABASE_ANON_KEY}
  SupabaseJwtSecret: ${INCTRAK_SUPABASE_JWT_SECRET}
```

These keys now belong in the YAML/env config when you want the API to resolve tenant and user data from the control-plane database rather than relying only on trusted header overrides.

## Open Design Questions

- should one email/user be allowed to belong to multiple tenants in v1
- should the first tenant admin also be represented immediately inside the tenant database, or only in the control-plane until explicit bootstrap work runs
- should participant membership be invite-only at first, or allow self-claiming by email domain later
- should custom domains be supported in the first tenant-domain schema, or only the default subdomain path
- should Supabase-hosted auth UI or custom frontend UI be the first integration path
