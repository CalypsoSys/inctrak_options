using System;
using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public class HeaderUserResolver : IUserResolver
    {
        public UserContext ResolveUser(HttpContext httpContext)
        {
            var context = new UserContext();
            if (httpContext == null)
            {
                return context;
            }

            if (httpContext.Request.Headers.TryGetValue(ControlPlaneHeaders.UserId, out var userId) &&
                Guid.TryParse(userId.ToString(), out Guid parsedUserId))
            {
                context.UserId = parsedUserId;
            }

            if (httpContext.Request.Headers.TryGetValue(ControlPlaneHeaders.UserRole, out var userRole))
            {
                context.Role = MembershipRoleParser.Parse(userRole.ToString());
            }

            if (httpContext.Request.Headers.TryGetValue(ControlPlaneHeaders.UserExternalIdentity, out var externalIdentity))
            {
                context.ExternalIdentity = externalIdentity.ToString().Trim();
            }

            return context;
        }
    }
}
