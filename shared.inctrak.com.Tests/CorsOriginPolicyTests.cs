using Xunit;

namespace inctrak.com.Tests
{
    public class CorsOriginPolicyTests
    {
        [Theory]
        [InlineData("https://inctrak.com")]
        [InlineData("https://www.inctrak.com")]
        [InlineData("https://shared.inctrak.com")]
        [InlineData("https://localhost:8080")]
        [InlineData("http://127.0.0.1:5500")]
        public void IsAllowedOrigin_AllowsConfiguredAndLocalOrigins(string origin)
        {
            Assert.True(CorsOriginPolicy.IsAllowedOrigin(origin));
        }

        [Theory]
        [InlineData("https://example.com")]
        [InlineData("file://local")]
        [InlineData("not-an-origin")]
        public void IsAllowedOrigin_RejectsUnexpectedOrigins(string origin)
        {
            Assert.False(CorsOriginPolicy.IsAllowedOrigin(origin));
        }
    }
}
