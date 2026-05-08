using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public class HeaderMembershipResolver : IMembershipResolver
    {
        public MembershipRole ResolveMembershipRole(HttpContext httpContext, TenantContext tenantContext, UserContext userContext)
        {
            if (tenantContext?.IsResolved() != true || userContext?.IsResolved() != true)
            {
                return MembershipRole.None;
            }

            return userContext.Role;
        }
    }
}
