using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace IncTrak.Data
{
    public static class PublicVestingUsageFormatter
    {
        public static string BuildSlackMessage(PublicVestingUsageEvent usageEvent)
        {
            if (usageEvent == null)
                throw new ArgumentNullException(nameof(usageEvent));

            var lines = new List<string>
            {
                "Public vesting usage",
                string.Format("Type: {0}", Clean(usageEvent.EventType, "unknown")),
                string.Format("Path: {0}", Clean(usageEvent.Path, "unknown")),
                string.Format("Success: {0}", usageEvent.Success ? "yes" : "no")
            };

            if (string.IsNullOrWhiteSpace(usageEvent.Provider) == false)
                lines.Add(string.Format("Provider: {0}", usageEvent.Provider.Trim()));

            if (string.IsNullOrWhiteSpace(usageEvent.AlternateProvider) == false)
                lines.Add(string.Format("Alternate: {0}", usageEvent.AlternateProvider.Trim()));

            if (usageEvent.Confidence.HasValue)
                lines.Add(string.Format("Confidence: {0:P0}", usageEvent.Confidence.Value));

            if (usageEvent.RequiresAi)
                lines.Add("Requires AI: yes");

            if (usageEvent.UsedAi)
                lines.Add("Used AI: yes");

            if (usageEvent.StrictAi)
                lines.Add("Strict AI: yes");

            if (string.IsNullOrWhiteSpace(usageEvent.PreferredProvider) == false)
                lines.Add(string.Format("Preferred provider: {0}", usageEvent.PreferredProvider.Trim()));

            if (string.IsNullOrWhiteSpace(usageEvent.Kind) == false)
                lines.Add(string.Format("Kind: {0}", usageEvent.Kind.Trim()));

            if (usageEvent.SharesGranted.HasValue)
                lines.Add(string.Format("Shares: {0}", usageEvent.SharesGranted.Value));

            if (string.IsNullOrWhiteSpace(usageEvent.VestingStart) == false)
                lines.Add(string.Format("Vesting start: {0}", usageEvent.VestingStart.Trim()));

            if (usageEvent.PeriodCount > 0)
                lines.Add(string.Format("Periods: {0}", usageEvent.PeriodCount));

            if (string.IsNullOrWhiteSpace(usageEvent.Prompt) == false)
                lines.Add(string.Format("Prompt: {0}", Truncate(FileLogWriter.SanitizeSingleLine(usageEvent.Prompt), 280)));

            if (string.IsNullOrWhiteSpace(usageEvent.Message) == false)
                lines.Add(string.Format("Message: {0}", Truncate(FileLogWriter.SanitizeSingleLine(usageEvent.Message), 200)));

            string maskedIp = MaskIpAddress(usageEvent.SourceIp);
            if (string.IsNullOrWhiteSpace(maskedIp) == false)
                lines.Add(string.Format("Source: {0}", maskedIp));

            if (string.IsNullOrWhiteSpace(usageEvent.UserAgent) == false)
                lines.Add(string.Format("User-Agent: {0}", Truncate(FileLogWriter.SanitizeSingleLine(usageEvent.UserAgent), 120)));

            return string.Join("\n", lines);
        }

        public static string MaskIpAddress(string rawIp)
        {
            if (string.IsNullOrWhiteSpace(rawIp))
                return string.Empty;

            string firstValue = rawIp.Split(',').Select(part => part.Trim()).FirstOrDefault(part => string.IsNullOrWhiteSpace(part) == false);
            if (string.IsNullOrWhiteSpace(firstValue))
                return string.Empty;

            if (IPAddress.TryParse(firstValue, out IPAddress ipAddress) == false)
                return FileLogWriter.SanitizeSingleLine(firstValue);

            byte[] bytes = ipAddress.GetAddressBytes();
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && bytes.Length == 4)
                return string.Format("{0}.{1}.{2}.x", bytes[0], bytes[1], bytes[2]);

            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && bytes.Length == 16)
            {
                var groups = new ushort[8];
                Buffer.BlockCopy(bytes, 0, groups, 0, bytes.Length);
                return string.Format(
                    "{0:x}:{1:x}:{2:x}:{3:x}:x:x:x:x",
                    IPAddress.NetworkToHostOrder((short)groups[0]) & 0xffff,
                    IPAddress.NetworkToHostOrder((short)groups[1]) & 0xffff,
                    IPAddress.NetworkToHostOrder((short)groups[2]) & 0xffff,
                    IPAddress.NetworkToHostOrder((short)groups[3]) & 0xffff);
            }

            return FileLogWriter.SanitizeSingleLine(firstValue);
        }

        private static string Clean(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            return FileLogWriter.SanitizeSingleLine(value.Trim());
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, Math.Max(0, maxLength - 3)) + "...";
        }
    }
}
