using IncTrak.Data;
using Microsoft.AspNetCore.Mvc;

namespace inctrak.com.Controllers
{
    [Route("api/control-plane")]
    [ApiController]
    public class ControlPlaneController : ControllerBase
    {
        private readonly RequestContextAccessor _requestContextAccessor;

        public ControlPlaneController(RequestContextAccessor requestContextAccessor)
        {
            _requestContextAccessor = requestContextAccessor;
        }

        [HttpGet("tenant-access")]
        [RequireMembershipRole(MembershipRole.TenantParticipant)]
        public IActionResult TenantAccess()
        {
            TenantContext tenantContext = _requestContextAccessor.GetTenantContext(HttpContext);
            UserContext userContext = _requestContextAccessor.GetUserContext(HttpContext);

            return Ok(new
            {
                success = true,
                TenantId = tenantContext.TenantId,
                TenantSlug = tenantContext.TenantSlug,
                UserId = userContext.UserId,
                Role = userContext.Role.ToString()
            });
        }

        [HttpGet("tenant-admin-access")]
        [RequireMembershipRole(MembershipRole.TenantAdmin)]
        public IActionResult TenantAdminAccess()
        {
            TenantContext tenantContext = _requestContextAccessor.GetTenantContext(HttpContext);
            UserContext userContext = _requestContextAccessor.GetUserContext(HttpContext);

            return Ok(new
            {
                success = true,
                TenantId = tenantContext.TenantId,
                TenantSlug = tenantContext.TenantSlug,
                TenantDatabaseName = tenantContext.TenantDatabaseName,
                UserId = userContext.UserId,
                Role = userContext.Role.ToString()
            });
        }
    }
}
