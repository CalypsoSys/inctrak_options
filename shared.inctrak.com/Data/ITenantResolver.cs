using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public interface ITenantResolver
    {
        TenantContext ResolveTenant(HttpContext httpContext);
    }
}
