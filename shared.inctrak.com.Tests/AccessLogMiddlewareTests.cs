using IncTrak.Data;
using IncTrak.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace inctrak.com.Tests
{
    public class AccessLogMiddlewareTests
    {
        [Fact]
        public async Task Invoke_WritesAccessLogLine()
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "inctrak-accesslog-tests", Guid.NewGuid().ToString("N"));
            string logPath = Path.Combine(tempRoot, "access.log");

            try
            {
                var settings = new AppSettings
                {
                    AccessLogPath = logPath
                };
                var middleware = new AccessLogMiddleware(
                    async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status201Created;
                        context.Response.ContentLength = 42;
                        await Task.CompletedTask;
                    },
                    Options.Create(settings));

                var context = new DefaultHttpContext();
                context.Request.Method = "POST";
                context.Request.Path = "/api/feedback/save_message/";
                context.Request.QueryString = new QueryString("?x=1");
                context.Request.Protocol = "HTTP/1.1";
                context.Request.Headers["Referer"] = "https://inctrak.com/contact";
                context.Request.Headers["User-Agent"] = "UnitTestAgent/1.0";
                context.Request.Headers["CF-Connecting-IP"] = "203.0.113.10";

                await middleware.Invoke(context);

                Assert.True(File.Exists(logPath));
                string logContents = File.ReadAllText(logPath);
                Assert.Contains("203.0.113.10 - - [", logContents);
                Assert.Contains("\"POST /api/feedback/save_message/?x=1 HTTP/1.1\" 201 42", logContents);
                Assert.Contains("\"https://inctrak.com/contact\"", logContents);
                Assert.Contains("\"UnitTestAgent/1.0\"", logContents);
                Assert.Contains("ms", logContents);
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, true);
            }
        }

        [Fact]
        public void BuildLogPath_RedactsSensitiveQueryParameters()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/login/resetpasswordlink/";
            context.Request.QueryString = new QueryString("?code=abc123&x=1&ResetPasswordKey=secret-value");

            string path = AccessLogMiddleware.BuildLogPath(context.Request);

            Assert.Contains("/api/login/resetpasswordlink/?", path);
            Assert.Contains("code=[REDACTED]", path);
            Assert.Contains("ResetPasswordKey=[REDACTED]", path);
            Assert.Contains("x=1", path);
            Assert.DoesNotContain("abc123", path);
            Assert.DoesNotContain("secret-value", path);
        }
    }
}
