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

        [Fact]
        public void BuildFeedbackSlackMessage_FormatsFeedbackOnlyPayload()
        {
            string message = IncTrakController.BuildFeedbackSlackMessage(
                "Jane Doe",
                "jane@example.test",
                "General Feedback",
                "ID: 203.0.113.1 User: abc",
                "Please add exports.");

            Assert.Contains("New feedback submitted", message);
            Assert.Contains("Type: General Feedback", message);
            Assert.Contains("Name: Jane Doe", message);
            Assert.Contains("Email: jane@example.test", message);
            Assert.Contains("Message: Please add exports.", message);
        }

        private sealed class TestIncTrakController : IncTrakController
        {
            public TestIncTrakController(IOptions<AppSettings> options) : base(options)
            {
            }

            public static string BuildSlackMessage(string to, string subject, string body)
            {
                return BuildSlackMailMessage(to, subject, body);
            }
        }
    }
}
