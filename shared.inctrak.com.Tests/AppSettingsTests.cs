using IncTrak.Data;
using Xunit;

namespace inctrak.com.Tests
{
    public class AppSettingsTests
    {
        [Fact]
        public void GetAccessLogPath_ReturnsConfiguredValue()
        {
            var settings = new AppSettings
            {
                AccessLogPath = "/home/joe/dotnet/mma/output/access.log"
            };

            Assert.Equal("/home/joe/dotnet/mma/output/access.log", settings.GetAccessLogPath());
        }

        [Fact]
        public void GetSlackFeedbackWebhookUrl_ReturnsConfiguredValue()
        {
            var settings = new AppSettings
            {
                SlackFeedbackWebhookUrl = "https://hooks.slack.test/services/example"
            };

            Assert.Equal("https://hooks.slack.test/services/example", settings.GetSlackFeedbackWebhookUrl());
        }

        [Fact]
        public void RateLimit_HasExpectedDefaults()
        {
            var settings = new AppSettings();

            Assert.False(settings.RateLimit.Enabled);
            Assert.Equal(120, settings.RateLimit.PermitLimit);
            Assert.Equal(60, settings.RateLimit.WindowSeconds);
            Assert.Equal(0, settings.RateLimit.QueueLimit);
        }
    }
}
