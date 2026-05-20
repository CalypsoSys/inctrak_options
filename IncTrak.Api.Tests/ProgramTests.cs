using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace inctrak.com.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void CreateHostBuilder_DoesNotConfigureWebRoot()
        {
            using var host = Program.CreateHostBuilder(System.Array.Empty<string>()).Build();
            IWebHostEnvironment environment = host.Services.GetRequiredService<IWebHostEnvironment>();

            Assert.True(string.IsNullOrWhiteSpace(environment.WebRootPath));
        }
    }
}
