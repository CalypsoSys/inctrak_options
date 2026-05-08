using IncTrak.Controllers;
using IncTrak.Data;
using IncTrak.FeedbackModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Xunit;

namespace inctrak.com.Tests
{
    public class FeedbackControllerTests
    {
        [Fact]
        public void Post_ReturnsGracefulFailure_WhenStorageAndSlackAreUnavailable()
        {
            var controller = new FeedbackController(Options.Create(new AppSettings
            {
                FeedbackConnection = "Host=localhost;Port=1;Database=inctrak_feedback;Username=postgres;Password=test"
            }));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = "http://127.0.0.1:5176";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            ActionResult result = controller.Post(new Feedback
            {
                EmailAddress = "founder@example.test",
                Name = "Founder",
                Subject = "Need help with vesting",
                Message = "Please contact me.",
                MessageTypeFk = 7
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsAssignableFrom<IDictionary<string, object>>(ToDictionary(ok.Value));
            Assert.Equal(false, payload["success"]);
            Assert.Equal("Unable to submit your message right now.", payload["message"]);
        }

        [Fact]
        public void Post_NormalizesCreatedTimestampToUtc()
        {
            var controller = new FeedbackController(Options.Create(new AppSettings
            {
                FeedbackConnection = "Host=localhost;Port=1;Database=inctrak_feedback;Username=postgres;Password=test"
            }));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = "http://127.0.0.1:5176";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var feedback = new Feedback
            {
                EmailAddress = "founder@example.test",
                Name = "Founder",
                Subject = "Need help with vesting",
                Message = "Please contact me.",
                MessageTypeFk = 7
            };

            controller.Post(feedback);

            Assert.Equal(System.DateTimeKind.Utc, feedback.Created.Kind);
        }

        private static IDictionary<string, object> ToDictionary(object value)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var property in value.GetType().GetProperties())
            {
                dictionary[property.Name] = property.GetValue(value);
            }

            return dictionary;
        }
    }
}
