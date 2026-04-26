using IncTrak.Controllers;
using IncTrak.Data;
using System;
using System.IO;
using Xunit;

namespace inctrak.com.Tests
{
    public class IncTrakErrorsTests
    {
        [Fact]
        public void LogError_WritesToConfiguredErrorLog()
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "inctrak-errors-tests", Guid.NewGuid().ToString("N"));
            string logPath = Path.Combine(tempRoot, "errors.log");

            try
            {
                var settings = new AppSettings
                {
                    ErrorLogPath = logPath
                };
                var login = new LoginRights(
                    "user-123",
                    false,
                    Guid.Empty,
                    Guid.Empty,
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Guid.Empty);
                var excp = new InvalidOperationException("outer failure", new Exception("inner failure"));

                string result = IncTrakErrors.LogError(settings, login, excp, "failed to save {0}", "grant");

                Assert.StartsWith("Unknown error occured code: [", result);
                Assert.True(File.Exists(logPath));

                string logContents = File.ReadAllText(logPath);
                Assert.Contains("message=failed to save grant", logContents);
                Assert.Contains("uuid=user-123", logContents);
                Assert.Contains("userKey=11111111-1111-1111-1111-111111111111", logContents);
                Assert.Contains("exception: outer failure", logContents);
                Assert.Contains("inner_exception_1: inner failure", logContents);
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, true);
            }
        }
    }
}
