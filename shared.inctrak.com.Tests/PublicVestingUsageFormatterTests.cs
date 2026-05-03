using IncTrak.Data;
using Xunit;

namespace inctrak.com.Tests
{
    public class PublicVestingUsageFormatterTests
    {
        [Fact]
        public void MaskIpAddress_MasksIpv4Address()
        {
            string masked = PublicVestingUsageFormatter.MaskIpAddress("203.0.113.42");

            Assert.Equal("203.0.113.x", masked);
        }

        [Fact]
        public void BuildSlackMessage_IncludesKeyAuditFields()
        {
            string message = PublicVestingUsageFormatter.BuildSlackMessage(new PublicVestingUsageEvent
            {
                EventType = "interpret",
                Path = "/api/optionee/quick/interpret/",
                Prompt = "Create a three-year quarterly vesting schedule for 100000 shares with vest start date 1/1/2022.",
                Provider = "parser",
                AlternateProvider = "pattern",
                Confidence = 0.92m,
                RequiresAi = false,
                UsedAi = false,
                Success = true,
                Message = "Interpreted vesting prompt.",
                Kind = "PeriodicNoCliff",
                SharesGranted = 100000m,
                VestingStart = "2022-01-01",
                PeriodCount = 1,
                SourceIp = "198.51.100.27",
                UserAgent = "Mozilla/5.0 Test Browser"
            });

            Assert.Contains("Public vesting usage", message);
            Assert.Contains("Type: interpret", message);
            Assert.Contains("Provider: parser", message);
            Assert.Contains("Alternate: pattern", message);
            Assert.Contains("Confidence:", message);
            Assert.Contains("Shares: 100000", message);
            Assert.Contains("Vesting start: 2022-01-01", message);
            Assert.Contains("Periods: 1", message);
            Assert.Contains("Source: 198.51.100.x", message);
            Assert.Contains("Prompt: Create a three-year quarterly vesting schedule", message);
        }
    }
}
