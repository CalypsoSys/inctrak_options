using System;

namespace inctrak.com
{
    public static class CorsOriginPolicy
    {
        public static readonly string[] DefaultAllowedOrigins =
        {
            "https://inctrak.com",
            "https://www.inctrak.com",
            "https://shared.inctrak.com",
            "http://localhost:5173",
            "https://localhost:5173",
            "https://localhost:8080",
            "http://127.0.0.1:5500"
        };

        public static bool IsAllowedOrigin(string origin)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri parsedOrigin))
            {
                return false;
            }

            if (!string.Equals(parsedOrigin.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(parsedOrigin.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (parsedOrigin.IsLoopback)
            {
                return true;
            }

            foreach (string allowedOrigin in DefaultAllowedOrigins)
            {
                if (string.Equals(origin, allowedOrigin, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
