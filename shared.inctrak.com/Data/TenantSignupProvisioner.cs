using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace IncTrak.Data
{
    public class TenantSignupProvisioner : ITenantSignupProvisioner
    {
        private static readonly Regex SlugCleanupRegex = new Regex("[^a-z0-9-]+", RegexOptions.Compiled);
        private static readonly Regex HyphenCollapseRegex = new Regex("-{2,}", RegexOptions.Compiled);

        private readonly AppSettings _settings;

        public TenantSignupProvisioner(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public TenantSignupResult ProvisionInitialTenant(SupabaseIdentity identity, TenantSignupRequest request)
        {
            if (identity == null || identity.IsAuthenticated() == false || string.IsNullOrWhiteSpace(identity.EmailAddress))
            {
                throw new InvalidOperationException("An authenticated Supabase user is required.");
            }

            string companyName = request?.CompanyName?.Trim();
            string normalizedSlug = NormalizeSlugForUi(request?.TenantSlug);
            if (string.IsNullOrWhiteSpace(companyName))
            {
                throw new InvalidOperationException("Company name is required.");
            }

            if (string.IsNullOrWhiteSpace(normalizedSlug))
            {
                throw new InvalidOperationException("Tenant slug is required.");
            }

            if (Guid.TryParse(identity.ExternalIdentity, out Guid supabaseUserId) == false)
            {
                throw new InvalidOperationException("The authenticated Supabase identity is missing a usable user id.");
            }

            string emailAddress = identity.EmailAddress.Trim().ToLowerInvariant();
            string tenantDatabaseName = BuildTenantDatabaseName(normalizedSlug);
            string primaryDomain = $"{normalizedSlug}.inctrak.com";

            using var connection = new NpgsqlConnection(_settings.GetControlPlaneConnection());
            connection.Open();

            EnsureSlugIsAvailable(connection, normalizedSlug);

            Guid controlPlaneUserId = UpsertControlPlaneUser(connection, supabaseUserId, emailAddress, companyName);
            TenantSignupResult existingTenant = TryGetExistingTenantForUser(connection, controlPlaneUserId, normalizedSlug);
            if (existingTenant != null)
            {
                return existingTenant;
            }

            EnsureTenantSlugDoesNotExist(connection, normalizedSlug);
            EnsureDatabaseNameDoesNotExist(tenantDatabaseName);

            CreateTenantDatabaseFromTemplate(tenantDatabaseName);

            Guid tenantId;
            using (var transaction = connection.BeginTransaction())
            {
                tenantId = InsertTenant(connection, transaction, companyName, normalizedSlug, primaryDomain, tenantDatabaseName);
                InsertTenantDomain(connection, transaction, tenantId, primaryDomain);
                InsertMembership(connection, transaction, tenantId, controlPlaneUserId);
                InsertProvisioningJob(connection, transaction, tenantId, controlPlaneUserId);
                transaction.Commit();
            }

            SeedInitialTenantAdmin(tenantDatabaseName, companyName, normalizedSlug, emailAddress);

            return new TenantSignupResult
            {
                TenantId = tenantId,
                TenantSlug = normalizedSlug,
                TenantDatabaseName = tenantDatabaseName,
                Created = true
            };
        }

        private void EnsureSlugIsAvailable(NpgsqlConnection connection, string normalizedSlug)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
select 1
from cp_reserved_slugs
where lower(slug_value) = lower(@slug)
limit 1;";
            command.Parameters.AddWithValue("slug", normalizedSlug);

            if (command.ExecuteScalar() != null)
            {
                throw new InvalidOperationException("That company slug is reserved. Please choose another one.");
            }
        }

        private Guid UpsertControlPlaneUser(NpgsqlConnection connection, Guid supabaseUserId, string emailAddress, string companyName)
        {
            string displayName = BuildDisplayName(emailAddress, companyName);
            using var command = connection.CreateCommand();
            command.CommandText = @"
insert into cp_users (
    supabase_user_id,
    email_address,
    display_name
)
values (
    @supabase_user_id,
    @email_address,
    @display_name
)
on conflict (supabase_user_id) do update
set
    email_address = excluded.email_address,
    display_name = coalesce(cp_users.display_name, excluded.display_name)
returning user_pk;";
            command.Parameters.AddWithValue("supabase_user_id", supabaseUserId);
            command.Parameters.AddWithValue("email_address", emailAddress);
            command.Parameters.AddWithValue("display_name", displayName);

            return (Guid)command.ExecuteScalar();
        }

        private TenantSignupResult TryGetExistingTenantForUser(NpgsqlConnection connection, Guid controlPlaneUserId, string normalizedSlug)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
select t.tenant_pk, t.tenant_slug, t.tenant_db_name
from cp_memberships m
join cp_tenants t on t.tenant_pk = m.tenant_fk
where m.user_fk = @user_fk
  and lower(m.role_code) = lower('tenant_admin')
  and lower(m.status) = lower('Active')
  and lower(t.tenant_slug) = lower(@tenant_slug)
limit 1;";
            command.Parameters.AddWithValue("user_fk", controlPlaneUserId);
            command.Parameters.AddWithValue("tenant_slug", normalizedSlug);

            using var reader = command.ExecuteReader();
            if (reader.Read() == false)
            {
                return null;
            }

            return new TenantSignupResult
            {
                TenantId = reader.GetGuid(0),
                TenantSlug = reader.GetString(1),
                TenantDatabaseName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Created = false
            };
        }

        private void EnsureTenantSlugDoesNotExist(NpgsqlConnection connection, string normalizedSlug)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
select 1
from cp_tenants
where lower(tenant_slug) = lower(@tenant_slug)
limit 1;";
            command.Parameters.AddWithValue("tenant_slug", normalizedSlug);

            if (command.ExecuteScalar() != null)
            {
                throw new InvalidOperationException("That company slug is already in use.");
            }
        }

        private void EnsureDatabaseNameDoesNotExist(string tenantDatabaseName)
        {
            using var adminConnection = CreateTenantAdminConnection();
            adminConnection.Open();
            using var command = adminConnection.CreateCommand();
            command.CommandText = "select 1 from pg_database where datname = @database_name limit 1;";
            command.Parameters.AddWithValue("database_name", tenantDatabaseName);

            if (command.ExecuteScalar() != null)
            {
                throw new InvalidOperationException("The tenant database name already exists.");
            }
        }

        private void CreateTenantDatabaseFromTemplate(string tenantDatabaseName)
        {
            using var adminConnection = CreateTenantAdminConnection();
            adminConnection.Open();
            string templateDatabaseName = GetTemplateDatabaseName();

            using (var templateCommand = adminConnection.CreateCommand())
            {
                templateCommand.CommandText = "select 1 from pg_database where datname = @template_database_name limit 1;";
                templateCommand.Parameters.AddWithValue("template_database_name", templateDatabaseName);

                if (templateCommand.ExecuteScalar() == null)
                {
                    throw new InvalidOperationException($"The tenant template database '{templateDatabaseName}' does not exist. Create it from inctrak.db/inctrak.sql before provisioning tenants.");
                }
            }

            using var command = adminConnection.CreateCommand();
            command.CommandText = $"create database {QuoteIdentifier(tenantDatabaseName)} with template {QuoteIdentifier(templateDatabaseName)};";
            command.ExecuteNonQuery();
        }

        private Guid InsertTenant(NpgsqlConnection connection, NpgsqlTransaction transaction, string companyName, string normalizedSlug, string primaryDomain, string tenantDatabaseName)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
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
    @company_name,
    @tenant_slug,
    'Active',
    @primary_domain,
    @tenant_db_name,
    'inctrak-bootstrap-v1',
    now()
)
returning tenant_pk;";
            command.Parameters.AddWithValue("company_name", companyName);
            command.Parameters.AddWithValue("tenant_slug", normalizedSlug);
            command.Parameters.AddWithValue("primary_domain", primaryDomain);
            command.Parameters.AddWithValue("tenant_db_name", tenantDatabaseName);

            return (Guid)command.ExecuteScalar();
        }

        private void InsertTenantDomain(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid tenantId, string primaryDomain)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
insert into cp_tenant_domains (
    tenant_fk,
    host_name,
    is_primary,
    verification_status
)
values (
    @tenant_fk,
    @host_name,
    true,
    'Verified'
);";
            command.Parameters.AddWithValue("tenant_fk", tenantId);
            command.Parameters.AddWithValue("host_name", primaryDomain);
            command.ExecuteNonQuery();
        }

        private void InsertMembership(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid tenantId, Guid controlPlaneUserId)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
insert into cp_memberships (
    tenant_fk,
    user_fk,
    role_code,
    status,
    accepted
)
values (
    @tenant_fk,
    @user_fk,
    'tenant_admin',
    'Active',
    now()
);";
            command.Parameters.AddWithValue("tenant_fk", tenantId);
            command.Parameters.AddWithValue("user_fk", controlPlaneUserId);
            command.ExecuteNonQuery();
        }

        private void InsertProvisioningJob(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid tenantId, Guid controlPlaneUserId)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
insert into cp_provisioning_jobs (
    tenant_fk,
    job_type,
    status,
    attempt_count,
    requested_by_user_fk,
    started,
    finished,
    result_json
)
values (
    @tenant_fk,
    'initial_signup',
    'Completed',
    1,
    @requested_by_user_fk,
    now(),
    now(),
    '{""provisioned"":true}'
);";
            command.Parameters.AddWithValue("tenant_fk", tenantId);
            command.Parameters.AddWithValue("requested_by_user_fk", controlPlaneUserId);
            command.ExecuteNonQuery();
        }

        private void SeedInitialTenantAdmin(string tenantDatabaseName, string companyName, string normalizedSlug, string emailAddress)
        {
            using var tenantConnection = CreateTenantConnection(tenantDatabaseName);
            tenantConnection.Open();
            using var command = tenantConnection.CreateCommand();
            command.CommandText = @"
with created_group as (
    insert into groups (
        description,
        group_key
    )
    values (
        @company_name,
        @group_key
    )
    returning group_pk
)
insert into users (
    group_fk,
    user_name,
    email_address,
    administrator
)
select
    group_pk,
    @user_name,
    @email_address,
    true
from created_group;";
            command.Parameters.AddWithValue("company_name", companyName);
            command.Parameters.AddWithValue("group_key", normalizedSlug);
            command.Parameters.AddWithValue("user_name", emailAddress);
            command.Parameters.AddWithValue("email_address", emailAddress);
            command.ExecuteNonQuery();
        }

        private NpgsqlConnection CreateTenantAdminConnection()
        {
            string baseConnectionString = _settings.GetIncTrakConnection();
            if (string.IsNullOrWhiteSpace(baseConnectionString))
            {
                throw new InvalidOperationException("AppSettings.IncTrakConnection is required for tenant provisioning.");
            }

            var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
            {
                Database = "postgres"
            };

            return new NpgsqlConnection(builder.ConnectionString);
        }

        private string GetTemplateDatabaseName()
        {
            string templateDatabaseName = _settings.GetTenantTemplateDatabaseName();
            if (string.IsNullOrWhiteSpace(templateDatabaseName))
            {
                return "inctrak_template";
            }

            return templateDatabaseName.Trim();
        }

        public static string NormalizeSlugForUi(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalized = value.Trim().ToLowerInvariant().Replace(' ', '-').Replace('_', '-');
            normalized = SlugCleanupRegex.Replace(normalized, "-");
            normalized = HyphenCollapseRegex.Replace(normalized, "-").Trim('-');

            if (normalized.Length > 63)
            {
                normalized = normalized.Substring(0, 63).Trim('-');
            }

            return normalized;
        }

        private string BuildTenantDatabaseName(string normalizedSlug)
        {
            string prefix = string.IsNullOrWhiteSpace(_settings.GetTenantDatabasePrefix())
                ? "inctrak_"
                : _settings.GetTenantDatabasePrefix().Trim();

            return $"{prefix}{normalizedSlug}".ToLowerInvariant();
        }

        private static string BuildDisplayName(string emailAddress, string companyName)
        {
            string localPart = emailAddress.Split('@')[0];
            if (string.IsNullOrWhiteSpace(localPart))
            {
                return companyName;
            }

            return localPart;
        }

        private static string QuoteIdentifier(string identifier)
        {
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }
    }
}
