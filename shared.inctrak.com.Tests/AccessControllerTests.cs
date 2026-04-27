using IncTrak.Controllers;
using IncTrak.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;

namespace inctrak.com.Tests
{
    public class AccessControllerTests
    {
        [Fact]
        public void PostRegisterGoogle_ReturnsRetiredMessage()
        {
            var controller = new AccessController(Options.Create(new AppSettings()));

            var result = Assert.IsType<OkObjectResult>(controller.PostRegisterGoogle(new IncTrak.data.USER_UI()));

            Assert.False((bool)result.Value.GetType().GetProperty("success").GetValue(result.Value));
            Assert.Equal(
                "Google sign-in has been retired. Use the Supabase login flow instead.",
                (string)result.Value.GetType().GetProperty("message").GetValue(result.Value));
        }

        [Fact]
        public void LoginGoogleUser_RedirectsToRetiredMessage()
        {
            var controller = new AccessController(Options.Create(new AppSettings()));

            var result = controller.LoginGoogleUser("code", "state", "session");

            Assert.Contains("/#/auth/login?redirect=true&success=false&message=", result.Url);
            Assert.Contains("Google%20sign-in%20has%20been%20retired", result.Url);
        }
    }
}
