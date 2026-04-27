using System.Threading.Tasks;
using IncTrak.Data;
using Microsoft.AspNetCore.Http;

namespace IncTrak.Middleware
{
    public class ControlPlaneContextMiddleware
    {
        private readonly RequestDelegate _next;

        public ControlPlaneContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ITenantResolver tenantResolver, IUserResolver userResolver, RequestContextAccessor requestContextAccessor)
        {
            requestContextAccessor.SetTenantContext(context, tenantResolver.ResolveTenant(context));
            requestContextAccessor.SetUserContext(context, userResolver.ResolveUser(context));
            await _next(context);
        }
    }
}
