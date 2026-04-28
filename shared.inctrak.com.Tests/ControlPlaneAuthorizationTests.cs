using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IncTrak.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        [Fact]
        public async Task TenantAdminAccess_AllowsAdminResolvedFromControlPlaneStore()
        {
            using var server = new TestServer(CreateBuilder(new FakeControlPlaneStore
            {
                Tenant = new ControlPlaneTenantRecord
                {
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    TenantSlug = "calypsosys",
                    TenantDatabaseName = "inctrak_calypsosys"
                },
                User = new ControlPlaneUserRecord
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333"
                },
                MembershipRole = MembershipRole.TenantAdmin
            }));
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/control-plane/tenant-admin-access");
            request.Headers.Host = "calypsosys.inctrak.com";
            request.Headers.Add("X-IncTrak-User-External-Id", "33333333-3333-3333-3333-333333333333");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TenantAdminAccess_DeniesParticipantResolvedFromControlPlaneStore()
        {
            using var server = new TestServer(CreateBuilder(new FakeControlPlaneStore
            {
                Tenant = new ControlPlaneTenantRecord
                {
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    TenantSlug = "calypsosys",
                    TenantDatabaseName = "inctrak_calypsosys"
                },
                User = new ControlPlaneUserRecord
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333"
                },
                MembershipRole = MembershipRole.TenantParticipant
            }));
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/control-plane/tenant-admin-access");
            request.Headers.Host = "calypsosys.inctrak.com";
            request.Headers.Add("X-IncTrak-User-External-Id", "33333333-3333-3333-3333-333333333333");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TenantAdminAccess_AllowsAdminResolvedFromSupabaseBearerToken()
        {
            using var server = new TestServer(CreateBuilder(new FakeControlPlaneStore
            {
                Tenant = new ControlPlaneTenantRecord
                {
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    TenantSlug = "calypsosys",
                    TenantDatabaseName = "inctrak_calypsosys"
                },
                User = new ControlPlaneUserRecord
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333"
                },
                MembershipRole = MembershipRole.TenantAdmin
            }, new FakeSupabaseTokenValidator
            {
                Identity = new SupabaseIdentity
                {
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333",
                    EmailAddress = "founder@calypsosys.com"
                }
            }));
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/control-plane/tenant-admin-access");
            request.Headers.Host = "calypsosys.inctrak.com";
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AppSession_ReturnsFrontendRoleFromSupabaseBearerToken()
        {
            using var server = new TestServer(CreateBuilder(new FakeControlPlaneStore
            {
                Tenant = new ControlPlaneTenantRecord
                {
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    TenantSlug = "calypsosys",
                    TenantDatabaseName = "inctrak_calypsosys"
                },
                User = new ControlPlaneUserRecord
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333"
                },
                MembershipRole = MembershipRole.TenantParticipant
            }, new FakeSupabaseTokenValidator
            {
                Identity = new SupabaseIdentity
                {
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333",
                    EmailAddress = "optionee@calypsosys.com"
                }
            }));
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/control-plane/app-session");
            request.Headers.Host = "calypsosys.inctrak.com";
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

            HttpResponseMessage response = await client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"Role\":\"optionee\"", body);
        }

        [Fact]
        public async Task AppSession_ReturnsAdminRoleWhenMembershipResolvesToTenantAdmin()
        {
            using var server = new TestServer(CreateBuilder(new FakeControlPlaneStore
            {
                Tenant = new ControlPlaneTenantRecord
                {
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    TenantSlug = "calypsosys",
                    TenantDatabaseName = "inctrak_calypsosys"
                },
                User = new ControlPlaneUserRecord
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333"
                },
                MembershipRole = MembershipRole.TenantAdmin
            }, new FakeSupabaseTokenValidator
            {
                Identity = new SupabaseIdentity
                {
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333",
                    EmailAddress = "founder@calypsosys.com"
                }
            }));
            using HttpClient client = server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/control-plane/app-session");
            request.Headers.Host = "calypsosys.inctrak.com";
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

            HttpResponseMessage response = await client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"Role\":\"admin\"", body);
        }

        private static IWebHostBuilder CreateBuilder(IControlPlaneStore controlPlaneStore = null, ISupabaseTokenValidator tokenValidator = null)
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
                .ConfigureServices(services =>
                {
                    if (controlPlaneStore != null)
                    {
                        services.Replace(ServiceDescriptor.Singleton(controlPlaneStore));
                    }

                    if (tokenValidator != null)
                    {
                        services.Replace(ServiceDescriptor.Singleton(tokenValidator));
                    }
                })
                .UseStartup<Startup>();
        }

        private class FakeControlPlaneStore : IControlPlaneStore
        {
            public ControlPlaneTenantRecord Tenant { get; set; }
            public ControlPlaneUserRecord User { get; set; }
            public MembershipRole MembershipRole { get; set; }

            public ControlPlaneTenantRecord FindTenantByHostName(string hostName)
            {
                return Tenant;
            }

            public ControlPlaneUserRecord FindUserByExternalIdentity(string externalIdentity)
            {
                return User;
            }

            public MembershipRole FindMembershipRole(string tenantId, string userId)
            {
                return MembershipRole;
            }
        }

        private class FakeSupabaseTokenValidator : ISupabaseTokenValidator
        {
            public SupabaseIdentity Identity { get; set; }

            public Task<SupabaseIdentity> ValidateTokenAsync(string token)
            {
                return Task.FromResult(Identity ?? new SupabaseIdentity());
            }
        }
    }
}
