using System;
using IncTrak.Data;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace inctrak.com.Tests
{
    public class ControlPlaneResolverTests
    {
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
    }
}
