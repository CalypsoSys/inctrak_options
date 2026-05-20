using IncTrak.Data;
using Microsoft.AspNetCore.Mvc;

namespace inctrak.com.Controllers
{
    public class VersionController : ControllerBase
    {
        [HttpGet]
        [Route("api/version/")]
        public object GetVersion()
        {
            return new
            {
                success = true,
                backendVersion = AppVersion.Backend
            };
        }
    }
}
