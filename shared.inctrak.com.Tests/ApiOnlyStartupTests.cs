using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace inctrak.com.Tests
{
    public class ApiOnlyStartupTests
    {
        [Fact]
        public async Task ApiOnlyHost_DoesNotServeStaticFiles()
        {
            using var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            using HttpClient client = server.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/index.html");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApiOnlyHost_AllowsConfiguredCorsOrigins()
        {
            using var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Options, "/api/company/summary/");
            request.Headers.Add("Origin", "https://inctrak.com");
            request.Headers.Add("Access-Control-Request-Method", "GET");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.Contains("https://inctrak.com", response.Headers.GetValues("Access-Control-Allow-Origin"));
        }

        [Fact]
        public async Task ApiOnlyHost_RejectsRequestsWithoutGatewaySecretInProduction()
        {
            using var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Production")
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("AppSettings:RequireGatewaySecret", "true"),
                        new KeyValuePair<string, string?>("AppSettings:GatewaySecret", "super-secret")
                    });
                })
                .UseStartup<Startup>());
            using HttpClient client = server.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/company/summary/");
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ApiOnlyHost_AllowsRequestsWithGatewaySecretInProduction()
        {
            using var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Production")
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("AppSettings:RequireGatewaySecret", "true"),
                        new KeyValuePair<string, string?>("AppSettings:GatewaySecret", "super-secret"),
                        new KeyValuePair<string, string?>("AppSettings:GatewaySecretHeaderName", "X-Test-Gateway")
                    });
                })
                .UseStartup<Startup>());
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/company/summary/");
            request.Headers.Add("X-Test-Gateway", "super-secret");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ApiOnlyHost_RateLimitsRequestsWhenEnabled()
        {
            using var server = new TestServer(new WebHostBuilder()
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("AppSettings:RateLimit:Enabled", "true"),
                        new KeyValuePair<string, string?>("AppSettings:RateLimit:PermitLimit", "1"),
                        new KeyValuePair<string, string?>("AppSettings:RateLimit:WindowSeconds", "60"),
                        new KeyValuePair<string, string?>("AppSettings:RateLimit:QueueLimit", "0")
                    });
                })
                .UseStartup<Startup>());
            using HttpClient client = server.CreateClient();
            using var firstRequest = new HttpRequestMessage(HttpMethod.Get, "/api/company/summary/");
            firstRequest.Headers.Add("CF-Connecting-IP", "203.0.113.10");
            using var secondRequest = new HttpRequestMessage(HttpMethod.Get, "/api/company/summary/");
            secondRequest.Headers.Add("CF-Connecting-IP", "203.0.113.10");

            HttpResponseMessage firstResponse = await client.SendAsync(firstRequest);
            HttpResponseMessage secondResponse = await client.SendAsync(secondRequest);

            Assert.NotEqual(HttpStatusCode.TooManyRequests, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
            Assert.True(secondResponse.Headers.Contains("Retry-After"));
            Assert.Contains("60", secondResponse.Headers.GetValues("Retry-After"));
        }

        [Fact]
        public async Task ApiOnlyHost_DoesNotExposeLegacyInternalLoginEndpoint()
        {
            using var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/login/login_internal/");
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApiOnlyHost_DoesNotExposeLegacyGoogleLoginEndpoint()
        {
            using var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/login/login_google_user/");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApiOnlyHost_DoesNotExposeLegacyPasswordResetEndpoint()
        {
            using var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/login/resetpassword/");
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApiOnlyHost_QuickData_DoesNotRequireTenantDatabase()
        {
            using var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            using HttpClient client = server.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/optionee/quick/");
            string body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"success\":true", body, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("PeriodTypes", body);
            Assert.Contains("AmountTypes", body);
        }
    }
}
