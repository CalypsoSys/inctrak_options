-- Example local seed data for the IncTrak control-plane database.
-- Copy and adapt this file for local use rather than editing it in place.

begin;

insert into cp_users (
    supabase_user_id,
    email_address,
    display_name
)
values (
    '33333333-3333-3333-3333-333333333333'::uuid,
    'founder@calypsosys.com',
    'Founder Admin'
)
on conflict (supabase_user_id) do update
set
    email_address = excluded.email_address,
    display_name = excluded.display_name;

insert into cp_tenants (
    company_name,
    tenant_slug,
    status,
    primary_domain,
    tenant_db_name,
    template_version,
    activated
)
values (
    'Calypso Systems',
    'calypsosys',
    'Active',
    'calypsosys.inctrak.com',
    'inctrak_calypsosys',
    'template-bootstrap-v1',
    now()
)
on conflict (tenant_slug) do update
set
    company_name = excluded.company_name,
    status = excluded.status,
    primary_domain = excluded.primary_domain,
    tenant_db_name = excluded.tenant_db_name,
    template_version = excluded.template_version,
    activated = excluded.activated;

insert into cp_tenant_domains (
    tenant_fk,
    host_name,
    is_primary,
    verification_status
)
select
    tenant_pk,
    'calypsosys.inctrak.com',
    true,
    'Verified'
from cp_tenants
where tenant_slug = 'calypsosys'
on conflict (host_name) do update
set
    tenant_fk = excluded.tenant_fk,
    is_primary = excluded.is_primary,
    verification_status = excluded.verification_status;

insert into cp_memberships (
    tenant_fk,
    user_fk,
    role_code,
    status,
    accepted
)
select
    t.tenant_pk,
    u.user_pk,
    'tenant_admin',
    'Active',
    now()
from cp_tenants t
join cp_users u on u.supabase_user_id = '33333333-3333-3333-3333-333333333333'::uuid
where t.tenant_slug = 'calypsosys'
on conflict (tenant_fk, user_fk) do update
set
    role_code = excluded.role_code,
    status = excluded.status,
    accepted = coalesce(cp_memberships.accepted, excluded.accepted);

commit;
