using System;
using System.Threading.Tasks;
using IncTrak.Data;
using Microsoft.AspNetCore.Http;

namespace IncTrak.Middleware
{
    public class SupabaseAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SupabaseAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ISupabaseTokenValidator tokenValidator, RequestContextAccessor requestContextAccessor)
        {
            string authHeader = context.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authHeader) == false &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                string token = authHeader.Substring("Bearer ".Length).Trim();
                SupabaseIdentity identity = await tokenValidator.ValidateTokenAsync(token);
                if (identity.IsAuthenticated())
                {
                    requestContextAccessor.SetSupabaseIdentity(context, identity);
                }
            }

            await _next(context);
        }
    }
}
