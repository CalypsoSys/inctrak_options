using System;
using Microsoft.AspNetCore.Http;

namespace inctrak.com
{
    public static class RequestAuthReader
    {
        private const string UuidHeaderName = "X-IncTrak-UUID";

        // Cross-origin SPA requests carry the UUID in a header because browser cookies stay scoped to the static site origin.
        public static string GetUuid(HttpRequest request)
        {
            if (request == null)
            {
                return Guid.Empty.ToString();
            }

            if (request.Headers.TryGetValue(UuidHeaderName, out var headerUuid) && HasValue(headerUuid))
            {
                return headerUuid.ToString();
            }

            if (request.Cookies.TryGetValue("UUID", out string cookieUuid) && HasValue(cookieUuid))
            {
                return cookieUuid;
            }

            return Guid.Empty.ToString();
        }

        private static bool HasValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   !string.Equals(value, "not set", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(value, "null", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase);
        }
    }
}
