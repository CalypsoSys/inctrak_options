using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public class ControlPlaneUserResolver : IUserResolver
    {
        private readonly HeaderUserResolver _headerUserResolver;
        private readonly IControlPlaneStore _controlPlaneStore;
        private readonly RequestContextAccessor _requestContextAccessor;

        public ControlPlaneUserResolver(HeaderUserResolver headerUserResolver, IControlPlaneStore controlPlaneStore, RequestContextAccessor requestContextAccessor)
        {
            _headerUserResolver = headerUserResolver;
            _controlPlaneStore = controlPlaneStore;
            _requestContextAccessor = requestContextAccessor;
        }

        public UserContext ResolveUser(HttpContext httpContext)
        {
            UserContext headerContext = _headerUserResolver.ResolveUser(httpContext);
            if (headerContext.IsResolved() && headerContext.Role != MembershipRole.None)
            {
                return headerContext;
            }

            string externalIdentity = headerContext.ExternalIdentity;
            if (string.IsNullOrWhiteSpace(externalIdentity))
            {
                externalIdentity = _requestContextAccessor.GetSupabaseIdentity(httpContext).ExternalIdentity;
                headerContext.ExternalIdentity = externalIdentity;
            }

            if (string.IsNullOrWhiteSpace(externalIdentity))
            {
                return headerContext;
            }

            ControlPlaneUserRecord userRecord = _controlPlaneStore.FindUserByExternalIdentity(externalIdentity);
            if (userRecord == null)
            {
                return headerContext;
            }

            headerContext.UserId = userRecord.UserId;
            headerContext.ExternalIdentity = userRecord.ExternalIdentity;
            return headerContext;
        }
    }
}
