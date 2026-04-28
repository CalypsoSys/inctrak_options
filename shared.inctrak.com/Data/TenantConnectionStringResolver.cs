using Npgsql;

namespace IncTrak.Data
{
    public static class TenantConnectionStringResolver
    {
        public static string Resolve(string baseConnectionString)
        {
            if (string.IsNullOrWhiteSpace(baseConnectionString))
            {
                return baseConnectionString;
            }

            TenantContext tenantContext = RequestContextAccessor.GetAmbientTenantContext();
            if (tenantContext?.IsResolved() != true || string.IsNullOrWhiteSpace(tenantContext.TenantDatabaseName))
            {
                return baseConnectionString;
            }

            var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
            {
                Database = tenantContext.TenantDatabaseName.Trim()
            };

            return builder.ConnectionString;
        }
    }
}
