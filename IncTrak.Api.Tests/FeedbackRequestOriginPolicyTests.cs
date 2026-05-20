using System;
using IncTrak.Data;
using Xunit;

namespace inctrak.com.Tests
{
    public class FeedbackRequestOriginPolicyTests
    {
        [Fact]
        public void IsAllowedReferrer_AllowsIncTrakDomain()
        {
            Assert.True(FeedbackRequestOriginPolicy.IsAllowedReferrer(new Uri("https://vesting.inctrak.com/contact")));
        }

        [Fact]
        public void IsAllowedReferrer_AllowsLocalhostInDebug()
        {
            Assert.True(FeedbackRequestOriginPolicy.IsAllowedReferrer(new Uri("http://localhost:5176")));
        }

        [Fact]
        public void IsAllowedReferrer_AllowsLoopbackIpInDebug()
        {
            Assert.True(FeedbackRequestOriginPolicy.IsAllowedReferrer(new Uri("http://127.0.0.1:5176")));
        }

        [Fact]
        public void IsAllowedReferrer_RejectsUnrelatedDomain()
        {
            Assert.False(FeedbackRequestOriginPolicy.IsAllowedReferrer(new Uri("https://example.com/contact")));
        }
    }
}
