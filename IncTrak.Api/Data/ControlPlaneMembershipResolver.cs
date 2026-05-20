using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public class ControlPlaneMembershipResolver : IMembershipResolver
    {
        private readonly HeaderMembershipResolver _headerMembershipResolver;
        private readonly IControlPlaneStore _controlPlaneStore;

        public ControlPlaneMembershipResolver(HeaderMembershipResolver headerMembershipResolver, IControlPlaneStore controlPlaneStore)
        {
            _headerMembershipResolver = headerMembershipResolver;
            _controlPlaneStore = controlPlaneStore;
        }

        public MembershipRole ResolveMembershipRole(HttpContext httpContext, TenantContext tenantContext, UserContext userContext)
        {
            MembershipRole headerRole = _headerMembershipResolver.ResolveMembershipRole(httpContext, tenantContext, userContext);
            if (headerRole != MembershipRole.None)
            {
                return headerRole;
            }

            if (tenantContext?.IsResolved() != true || userContext?.IsResolved() != true)
            {
                return MembershipRole.None;
            }

            return _controlPlaneStore.FindMembershipRole(tenantContext.TenantId.ToString(), userContext.UserId.ToString());
        }
    }
}
