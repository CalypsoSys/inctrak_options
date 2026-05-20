using System;
using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public class HeaderTenantResolver : ITenantResolver
    {
        public TenantContext ResolveTenant(HttpContext httpContext)
        {
            var context = new TenantContext();
            if (httpContext == null)
            {
                return context;
            }

            if (httpContext.Request.Headers.TryGetValue(ControlPlaneHeaders.TenantId, out var tenantId) &&
                Guid.TryParse(tenantId.ToString(), out Guid parsedTenantId))
            {
                context.TenantId = parsedTenantId;
            }

            if (httpContext.Request.Headers.TryGetValue(ControlPlaneHeaders.TenantSlug, out var tenantSlug))
            {
                context.TenantSlug = tenantSlug.ToString().Trim();
            }

            if (httpContext.Request.Headers.TryGetValue(ControlPlaneHeaders.TenantDatabaseName, out var tenantDbName))
            {
                context.TenantDatabaseName = tenantDbName.ToString().Trim();
            }

            return context;
        }
    }
}
