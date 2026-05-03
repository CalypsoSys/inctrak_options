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
                AccessLogPath = "/var/log/inctrak/access.log"
            };

            Assert.Equal("/var/log/inctrak/access.log", settings.GetAccessLogPath());
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
        public void GetControlPlaneConnection_ReturnsConfiguredValue()
        {
            var settings = new AppSettings
            {
                ControlPlaneConnection = "Host=localhost;Database=inctrak_control;"
            };

            Assert.Equal("Host=localhost;Database=inctrak_control;", settings.GetControlPlaneConnection());
        }

        [Fact]
        public void GetSupabaseSettings_ReturnConfiguredValues()
        {
            var settings = new AppSettings
            {
                SupabaseUrl = "https://example.supabase.co",
                SupabaseAnonKey = "sb_publishable_example",
                SupabaseJwtSecret = "legacy-secret"
            };

            Assert.Equal("https://example.supabase.co", settings.GetSupabaseUrl());
            Assert.Equal("sb_publishable_example", settings.GetSupabaseAnonKey());
            Assert.Equal("legacy-secret", settings.GetSupabaseJwtSecret());
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
