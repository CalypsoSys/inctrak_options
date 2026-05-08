using System;
using IncTrak.Data;
using Microsoft.AspNetCore.Mvc;

namespace inctrak.com.Controllers
{
    [Route("api/control-plane")]
    [ApiController]
    public class ControlPlaneController : ControllerBase
    {
        private readonly RequestContextAccessor _requestContextAccessor;
        private readonly ITenantSignupProvisioner _tenantSignupProvisioner;
        private readonly IControlPlaneStore _controlPlaneStore;

        public ControlPlaneController(RequestContextAccessor requestContextAccessor, ITenantSignupProvisioner tenantSignupProvisioner, IControlPlaneStore controlPlaneStore)
        {
            _requestContextAccessor = requestContextAccessor;
            _tenantSignupProvisioner = tenantSignupProvisioner;
            _controlPlaneStore = controlPlaneStore;
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

        [HttpGet("app-session")]
        [RequireMembershipRole(MembershipRole.TenantParticipant)]
        public IActionResult AppSession()
        {
            UserContext userContext = _requestContextAccessor.GetUserContext(HttpContext);

            return Ok(new
            {
                success = true,
                Role = userContext.Role == MembershipRole.TenantAdmin ? "admin" : "optionee"
            });
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] TenantSignupRequest request)
        {
            SupabaseIdentity identity = _requestContextAccessor.GetSupabaseIdentity(HttpContext);
            if (identity.IsAuthenticated() == false)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "You must sign in before provisioning a company workspace."
                });
            }

            try
            {
                TenantSignupResult result = _tenantSignupProvisioner.ProvisionInitialTenant(identity, request);
                return Ok(new
                {
                    success = true,
                    TenantId = result.TenantId,
                    TenantSlug = result.TenantSlug,
                    TenantDatabaseName = result.TenantDatabaseName,
                    Created = result.Created
                });
            }
            catch (InvalidOperationException excp)
            {
                return BadRequest(new
                {
                    success = false,
                    message = excp.Message
                });
            }
        }

        [HttpGet("slug-availability")]
        public IActionResult SlugAvailability([FromQuery] string tenantSlug)
        {
            string normalizedSlug = TenantSignupProvisioner.NormalizeSlugForUi(tenantSlug);
            if (string.IsNullOrWhiteSpace(normalizedSlug))
            {
                return Ok(new
                {
                    success = true,
                    TenantSlug = normalizedSlug,
                    Available = false,
                    Message = "Enter a company slug."
                });
            }

            bool available = _controlPlaneStore.IsTenantSlugAvailable(normalizedSlug);
            return Ok(new
            {
                success = true,
                TenantSlug = normalizedSlug,
                Available = available,
                Message = available ? "Company slug is available." : "That company slug is already in use or reserved."
            });
        }
    }
}
