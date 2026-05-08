using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public interface IMembershipResolver
    {
        MembershipRole ResolveMembershipRole(HttpContext httpContext, TenantContext tenantContext, UserContext userContext);
    }
}
