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

        [Fact]
        public void BuildSlackMailMessage_ConvertsHtmlToReadableText()
        {
            string message = TestIncTrakController.BuildSlackMessage(
                "feedback@inctrak.com",
                "IncTrak: Activate account link",
                "Hi Joe,<br/>Please <a href='https://example.test/activate'>click</a> the link.<br/><br/>Thanks");

            Assert.Contains("To: feedback@inctrak.com", message);
            Assert.Contains("Subject: IncTrak: Activate account link", message);
            Assert.Contains("Hi Joe,", message);
            Assert.Contains("Please click the link.", message);
            Assert.Contains("Thanks", message);
            Assert.DoesNotContain("<a href", message);
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

            public static string BuildSlackMessage(string to, string subject, string body)
            {
                return BuildSlackMailMessage(to, subject, body);
            }
        }
    }
}
