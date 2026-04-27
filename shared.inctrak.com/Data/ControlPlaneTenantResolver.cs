using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public class ControlPlaneTenantResolver : ITenantResolver
    {
        private readonly HeaderTenantResolver _headerTenantResolver;
        private readonly IControlPlaneStore _controlPlaneStore;

        public ControlPlaneTenantResolver(HeaderTenantResolver headerTenantResolver, IControlPlaneStore controlPlaneStore)
        {
            _headerTenantResolver = headerTenantResolver;
            _controlPlaneStore = controlPlaneStore;
        }

        public TenantContext ResolveTenant(HttpContext httpContext)
        {
            TenantContext headerContext = _headerTenantResolver.ResolveTenant(httpContext);
            if (headerContext.IsResolved())
            {
                return headerContext;
            }

            ControlPlaneTenantRecord tenantRecord = _controlPlaneStore.FindTenantByHostName(httpContext?.Request?.Host.Host);
            if (tenantRecord == null)
            {
                return headerContext;
            }

            return new TenantContext
            {
                TenantId = tenantRecord.TenantId,
                TenantSlug = tenantRecord.TenantSlug,
                TenantDatabaseName = tenantRecord.TenantDatabaseName
            };
        }
    }
}
