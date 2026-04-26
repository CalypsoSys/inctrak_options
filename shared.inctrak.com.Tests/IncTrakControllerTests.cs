using IncTrak.Controllers;
using IncTrak.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;

namespace inctrak.com.Tests
{
    public class IncTrakControllerTests
    {
        [Fact]
        public void GetIncTrakApiUrl_UsesCurrentRequestOrigin()
        {
            var controller = new TestIncTrakController(Options.Create(new AppSettings()));
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("localhost:5001");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            Assert.Equal("https://localhost:5001/api/login/test/", controller.GetLoginUrl("test"));
        }

        private sealed class TestIncTrakController : IncTrakController
        {
            public TestIncTrakController(IOptions<AppSettings> options) : base(options)
            {
            }

            public string GetLoginUrl(string redirect)
            {
                return LoginBaseUrl(redirect);
            }
        }
    }
}
