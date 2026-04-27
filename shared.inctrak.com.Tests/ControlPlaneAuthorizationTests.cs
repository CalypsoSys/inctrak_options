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
    public class ControlPlaneAuthorizationTests
    {
        [Fact]
        public async Task TenantAdminAccess_AllowsTenantAdmin()
        {
            using var server = new TestServer(CreateBuilder());
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/control-plane/tenant-admin-access");
            request.Headers.Add("X-IncTrak-Tenant-Id", "11111111-1111-1111-1111-111111111111");
            request.Headers.Add("X-IncTrak-Tenant-Slug", "calypsosys");
            request.Headers.Add("X-IncTrak-Tenant-Db", "inctrak_calypsosys");
            request.Headers.Add("X-IncTrak-User-Id", "22222222-2222-2222-2222-222222222222");
            request.Headers.Add("X-IncTrak-User-Role", "tenant_admin");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TenantAdminAccess_DeniesTenantParticipant()
        {
            using var server = new TestServer(CreateBuilder());
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/control-plane/tenant-admin-access");
            request.Headers.Add("X-IncTrak-Tenant-Id", "11111111-1111-1111-1111-111111111111");
            request.Headers.Add("X-IncTrak-Tenant-Slug", "calypsosys");
            request.Headers.Add("X-IncTrak-User-Id", "22222222-2222-2222-2222-222222222222");
            request.Headers.Add("X-IncTrak-User-Role", "tenant_participant");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TenantAccess_DeniesRequestsWithoutTenantContext()
        {
            using var server = new TestServer(CreateBuilder());
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/control-plane/tenant-access");
            request.Headers.Add("X-IncTrak-User-Id", "22222222-2222-2222-2222-222222222222");
            request.Headers.Add("X-IncTrak-User-Role", "tenant_admin");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static IWebHostBuilder CreateBuilder()
        {
            return new WebHostBuilder()
                .UseEnvironment("Development")
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("AppSettings:RateLimit:Enabled", "false")
                    });
                })
                .UseStartup<Startup>();
        }
    }
}
