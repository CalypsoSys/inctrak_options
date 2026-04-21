using Microsoft.AspNetCore.Http;
using Xunit;

namespace inctrak.com.Tests
{
    public class RequestAuthReaderTests
    {
        [Fact]
        public void GetUuid_PrefersHeaderUuid()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["X-IncTrak-UUID"] = "header-uuid";
            context.Request.Headers["Cookie"] = "UUID=cookie-uuid";

            Assert.Equal("header-uuid", RequestAuthReader.GetUuid(context.Request));
        }

        [Fact]
        public void GetUuid_FallsBackToCookieUuid()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Cookie"] = "UUID=cookie-uuid";

            Assert.Equal("cookie-uuid", RequestAuthReader.GetUuid(context.Request));
        }

        [Fact]
        public void GetUuid_ReturnsEmptyGuidWhenMissing()
        {
            var context = new DefaultHttpContext();

            Assert.Equal(System.Guid.Empty.ToString(), RequestAuthReader.GetUuid(context.Request));
        }
    }
}
