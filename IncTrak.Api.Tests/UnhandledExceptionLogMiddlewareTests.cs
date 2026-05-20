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
    public class UnhandledExceptionLogMiddlewareTests
    {
        [Fact]
        public async Task Invoke_WritesUnhandledExceptionToErrorLogAndRethrows()
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "inctrak-unhandled-tests", Guid.NewGuid().ToString("N"));
            string logPath = Path.Combine(tempRoot, "errors.log");

            try
            {
                var settings = new AppSettings
                {
                    ErrorLogPath = logPath
                };
                var middleware = new UnhandledExceptionLogMiddleware(
                    context => throw new InvalidOperationException("middleware failure"),
                    Options.Create(settings));

                var context = new DefaultHttpContext();
                context.Request.Method = "GET";
                context.Request.Path = "/api/optionee/quick/";

                InvalidOperationException excp = await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.Invoke(context));

                Assert.Equal("middleware failure", excp.Message);
                Assert.True(File.Exists(logPath));

                string logContents = File.ReadAllText(logPath);
                Assert.Contains("message=unhandled request GET /api/optionee/quick/", logContents);
                Assert.Contains("exception: middleware failure", logContents);
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, true);
            }
        }
    }
}
