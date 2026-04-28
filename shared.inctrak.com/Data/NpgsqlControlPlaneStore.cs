using System;
using Npgsql;

namespace IncTrak.Data
{
    public class NpgsqlControlPlaneStore : IControlPlaneStore
    {
        private readonly AppSettings _settings;

        public NpgsqlControlPlaneStore(Microsoft.Extensions.Options.IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public ControlPlaneTenantRecord FindTenantByHostName(string hostName)
        {
            if (string.IsNullOrWhiteSpace(_settings.GetControlPlaneConnection()) || string.IsNullOrWhiteSpace(hostName))
            {
                return null;
            }

            using var connection = new NpgsqlConnection(_settings.GetControlPlaneConnection());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
select t.tenant_pk, t.tenant_slug, t.tenant_db_name
from cp_tenant_domains d
join cp_tenants t on t.tenant_pk = d.tenant_fk
where lower(d.host_name) = lower(@host_name)
  and lower(t.status) = lower('Active')
limit 1;";
            command.Parameters.AddWithValue("host_name", hostName);

            using var reader = command.ExecuteReader();
            if (reader.Read() == false)
            {
                return null;
            }

            return new ControlPlaneTenantRecord
            {
                TenantId = reader.GetGuid(0),
                TenantSlug = reader.IsDBNull(1) ? null : reader.GetString(1),
                TenantDatabaseName = reader.IsDBNull(2) ? null : reader.GetString(2)
            };
        }

        public ControlPlaneUserRecord FindUserByExternalIdentity(string externalIdentity)
        {
            if (string.IsNullOrWhiteSpace(_settings.GetControlPlaneConnection()) ||
                Guid.TryParse(externalIdentity, out Guid parsedExternalIdentity) == false)
            {
                return null;
            }

            using var connection = new NpgsqlConnection(_settings.GetControlPlaneConnection());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
select user_pk, supabase_user_id
from cp_users
where supabase_user_id = @supabase_user_id
limit 1;";
            command.Parameters.AddWithValue("supabase_user_id", parsedExternalIdentity);

            using var reader = command.ExecuteReader();
            if (reader.Read() == false)
            {
                return null;
            }

            return new ControlPlaneUserRecord
            {
                UserId = reader.GetGuid(0),
                ExternalIdentity = reader.GetGuid(1).ToString()
            };
        }

        public MembershipRole FindMembershipRole(string tenantId, string userId)
        {
            if (string.IsNullOrWhiteSpace(_settings.GetControlPlaneConnection()) ||
                Guid.TryParse(tenantId, out Guid parsedTenantId) == false ||
                Guid.TryParse(userId, out Guid parsedUserId) == false)
            {
                return MembershipRole.None;
            }

            using var connection = new NpgsqlConnection(_settings.GetControlPlaneConnection());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
select role_code
from cp_memberships
where tenant_fk = @tenant_fk
  and user_fk = @user_fk
  and lower(status) = lower('Active')
limit 1;";
            command.Parameters.AddWithValue("tenant_fk", parsedTenantId);
            command.Parameters.AddWithValue("user_fk", parsedUserId);

            object value = command.ExecuteScalar();
            return MembershipRoleParser.Parse(value?.ToString());
        }

        public bool IsTenantSlugAvailable(string tenantSlug)
        {
            if (string.IsNullOrWhiteSpace(_settings.GetControlPlaneConnection()) || string.IsNullOrWhiteSpace(tenantSlug))
            {
                return false;
            }

            string normalizedSlug = tenantSlug.Trim();
            using var connection = new NpgsqlConnection(_settings.GetControlPlaneConnection());
            connection.Open();

            using (var reservedCommand = connection.CreateCommand())
            {
                reservedCommand.CommandText = @"
select 1
from cp_reserved_slugs
where lower(slug_value) = lower(@tenant_slug)
limit 1;";
                reservedCommand.Parameters.AddWithValue("tenant_slug", normalizedSlug);

                if (reservedCommand.ExecuteScalar() != null)
                {
                    return false;
                }
            }

            using var tenantCommand = connection.CreateCommand();
            tenantCommand.CommandText = @"
select 1
from cp_tenants
where lower(tenant_slug) = lower(@tenant_slug)
limit 1;";
            tenantCommand.Parameters.AddWithValue("tenant_slug", normalizedSlug);

            return tenantCommand.ExecuteScalar() == null;
        }
    }
}
