using System;

namespace IncTrak.Data
{
    public static class FeedbackRequestOriginPolicy
    {
        public static bool IsAllowedReferrer(Uri referrer)
        {
            if (referrer == null)
                return false;

            if (referrer.DnsSafeHost.EndsWith("inctrak.com", StringComparison.OrdinalIgnoreCase))
                return true;

#if DEBUG
            if (string.Equals(referrer.DnsSafeHost, "localhost", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(referrer.DnsSafeHost, "127.0.0.1", StringComparison.OrdinalIgnoreCase))
                return true;
#endif

            return false;
        }
    }
}
