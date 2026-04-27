using System;
using IncTrak.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IncTrak.Data
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireMembershipRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly MembershipRole _minimumRole;

        public RequireMembershipRoleAttribute(MembershipRole minimumRole)
        {
            _minimumRole = minimumRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requestContextAccessor = context.HttpContext.RequestServices.GetService(typeof(RequestContextAccessor)) as RequestContextAccessor;
            var membershipResolver = context.HttpContext.RequestServices.GetService(typeof(IMembershipResolver)) as IMembershipResolver;

            TenantContext tenantContext = requestContextAccessor?.GetTenantContext(context.HttpContext) ?? new TenantContext();
            UserContext userContext = requestContextAccessor?.GetUserContext(context.HttpContext) ?? new UserContext();

            if (tenantContext.IsResolved() == false || userContext.IsResolved() == false)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            MembershipRole resolvedRole = membershipResolver?.ResolveMembershipRole(context.HttpContext, tenantContext, userContext) ?? MembershipRole.None;
            if (HasRequiredRole(resolvedRole) == false)
            {
                context.Result = new StatusCodeResult(403);
            }
        }

        private bool HasRequiredRole(MembershipRole resolvedRole)
        {
            return _minimumRole switch
            {
                MembershipRole.TenantParticipant => resolvedRole == MembershipRole.TenantParticipant ||
                    resolvedRole == MembershipRole.TenantAdmin ||
                    resolvedRole == MembershipRole.PlatformOperator,
                MembershipRole.TenantAdmin => resolvedRole == MembershipRole.TenantAdmin ||
                    resolvedRole == MembershipRole.PlatformOperator,
                MembershipRole.PlatformOperator => resolvedRole == MembershipRole.PlatformOperator,
                _ => false
            };
        }
    }
}
