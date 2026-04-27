using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public class RequestContextAccessor
    {
        private static readonly object TenantContextKey = new object();
        private static readonly object UserContextKey = new object();
        private static readonly object SupabaseIdentityKey = new object();

        public TenantContext GetTenantContext(HttpContext httpContext)
        {
            if (httpContext?.Items.TryGetValue(TenantContextKey, out object value) == true &&
                value is TenantContext tenantContext)
            {
                return tenantContext;
            }

            return new TenantContext();
        }

        public UserContext GetUserContext(HttpContext httpContext)
        {
            if (httpContext?.Items.TryGetValue(UserContextKey, out object value) == true &&
                value is UserContext userContext)
            {
                return userContext;
            }

            return new UserContext();
        }

        public void SetTenantContext(HttpContext httpContext, TenantContext tenantContext)
        {
            httpContext.Items[TenantContextKey] = tenantContext;
        }

        public void SetUserContext(HttpContext httpContext, UserContext userContext)
        {
            httpContext.Items[UserContextKey] = userContext;
        }

        public SupabaseIdentity GetSupabaseIdentity(HttpContext httpContext)
        {
            if (httpContext?.Items.TryGetValue(SupabaseIdentityKey, out object value) == true &&
                value is SupabaseIdentity identity)
            {
                return identity;
            }

            return new SupabaseIdentity();
        }

        public void SetSupabaseIdentity(HttpContext httpContext, SupabaseIdentity identity)
        {
            httpContext.Items[SupabaseIdentityKey] = identity;
        }
    }
}
