using System;

namespace inctrak.com
{
    public static class CorsOriginPolicy
    {
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

            return string.Equals(origin, "https://inctrak.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(origin, "https://www.inctrak.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(origin, "https://shared.inctrak.com", StringComparison.OrdinalIgnoreCase);
        }
    }
}
