using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
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
            using var request = new HttpRequestMessage(HttpMethod.Options, "/api/login/get_creds/");
            request.Headers.Add("Origin", "https://inctrak.com");
            request.Headers.Add("Access-Control-Request-Method", "GET");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.Contains("https://inctrak.com", response.Headers.GetValues("Access-Control-Allow-Origin"));
        }
    }
}
