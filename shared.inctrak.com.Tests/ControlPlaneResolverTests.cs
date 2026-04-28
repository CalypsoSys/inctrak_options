using System;
using IncTrak.Data;
using Microsoft.AspNetCore.Http;
using Npgsql;
using Xunit;

namespace inctrak.com.Tests
{
    public class ControlPlaneResolverTests
    {
        [Fact]
        public void ResolveTenant_UsesControlPlaneStoreWhenTrustedHeadersAreMissing()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("calypsosys.inctrak.com");
            var resolver = new ControlPlaneTenantResolver(new HeaderTenantResolver(), new FakeControlPlaneStore
            {
                Tenant = new ControlPlaneTenantRecord
                {
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    TenantSlug = "calypsosys",
                    TenantDatabaseName = "inctrak_calypsosys"
                }
            });

            TenantContext tenantContext = resolver.ResolveTenant(context);

            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), tenantContext.TenantId);
            Assert.Equal("calypsosys", tenantContext.TenantSlug);
            Assert.Equal("inctrak_calypsosys", tenantContext.TenantDatabaseName);
        }

        [Fact]
        public void ResolveTenant_ReadsTrustedHeaders()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[ControlPlaneHeaders.TenantId] = "11111111-1111-1111-1111-111111111111";
            context.Request.Headers[ControlPlaneHeaders.TenantSlug] = "calypsosys";
            context.Request.Headers[ControlPlaneHeaders.TenantDatabaseName] = "inctrak_calypsosys";

            TenantContext tenantContext = new HeaderTenantResolver().ResolveTenant(context);

            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), tenantContext.TenantId);
            Assert.Equal("calypsosys", tenantContext.TenantSlug);
            Assert.Equal("inctrak_calypsosys", tenantContext.TenantDatabaseName);
        }

        [Fact]
        public void ResolveUser_UsesControlPlaneStoreWhenExternalIdentityHeaderIsPresent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[ControlPlaneHeaders.UserExternalIdentity] = "33333333-3333-3333-3333-333333333333";
            var resolver = new ControlPlaneUserResolver(new HeaderUserResolver(), new FakeControlPlaneStore
            {
                User = new ControlPlaneUserRecord
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333"
                }
            }, new RequestContextAccessor());

            UserContext userContext = resolver.ResolveUser(context);

            Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), userContext.UserId);
            Assert.Equal("33333333-3333-3333-3333-333333333333", userContext.ExternalIdentity);
            Assert.Equal(MembershipRole.None, userContext.Role);
        }

        [Fact]
        public void ResolveUser_ReadsRoleAndIdentityHeaders()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[ControlPlaneHeaders.UserId] = "22222222-2222-2222-2222-222222222222";
            context.Request.Headers[ControlPlaneHeaders.UserRole] = "tenant_admin";
            context.Request.Headers[ControlPlaneHeaders.UserExternalIdentity] = "supabase-user-1";

            UserContext userContext = new HeaderUserResolver().ResolveUser(context);

            Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), userContext.UserId);
            Assert.Equal(MembershipRole.TenantAdmin, userContext.Role);
            Assert.Equal("supabase-user-1", userContext.ExternalIdentity);
        }

        [Fact]
        public void ResolveUser_UsesSupabaseIdentityFromRequestContextWhenHeaderIsMissing()
        {
            var context = new DefaultHttpContext();
            var requestContextAccessor = new RequestContextAccessor();
            requestContextAccessor.SetSupabaseIdentity(context, new SupabaseIdentity
            {
                ExternalIdentity = "33333333-3333-3333-3333-333333333333",
                EmailAddress = "founder@calypsosys.com"
            });
            var resolver = new ControlPlaneUserResolver(new HeaderUserResolver(), new FakeControlPlaneStore
            {
                User = new ControlPlaneUserRecord
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ExternalIdentity = "33333333-3333-3333-3333-333333333333"
                }
            }, requestContextAccessor);

            UserContext userContext = resolver.ResolveUser(context);

            Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), userContext.UserId);
            Assert.Equal("33333333-3333-3333-3333-333333333333", userContext.ExternalIdentity);
        }

        [Fact]
        public void ResolveConnectionString_UsesTenantDatabaseFromAmbientContext()
        {
            var httpContext = new DefaultHttpContext();
            var requestContextAccessor = new RequestContextAccessor();
            requestContextAccessor.SetTenantContext(httpContext, new TenantContext
            {
                TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                TenantSlug = "calypsosys",
                TenantDatabaseName = "inctrak_calypsosys"
            });

            string resolved = TenantConnectionStringResolver.Resolve("Host=localhost;Port=5432;Database=inctrak;Username=postgres;Password=test");
            var builder = new NpgsqlConnectionStringBuilder(resolved);

            Assert.Equal("inctrak_calypsosys", builder.Database);
        }

        private class FakeControlPlaneStore : IControlPlaneStore
        {
            public ControlPlaneTenantRecord Tenant { get; set; }
            public ControlPlaneUserRecord User { get; set; }

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
                return MembershipRole.None;
            }

            public bool IsTenantSlugAvailable(string tenantSlug)
            {
                return true;
            }
        }
    }
}
