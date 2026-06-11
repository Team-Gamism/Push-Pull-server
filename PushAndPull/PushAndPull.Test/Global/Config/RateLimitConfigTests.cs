using System.Net;
using Microsoft.AspNetCore.Http;
using PushAndPull.Global.Config;

namespace Tests.Global.Config;

public class RateLimitConfigTests
{
    public class WhenResolvingTheIpPartitionKey
    {
        [Fact]
        public void It_UsesTheRemoteIpAddress()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");

            var key = RateLimitConfig.GetIpKey(httpContext);

            Assert.Equal("ip:203.0.113.10", key);
        }

        [Fact]
        public void It_FallsBackToUnknownWhenNoRemoteIpExists()
        {
            var httpContext = new DefaultHttpContext();

            var key = RateLimitConfig.GetIpKey(httpContext);

            Assert.Equal("ip:unknown", key);
        }
    }

    public class WhenResolvingTheSessionPartitionKey
    {
        [Fact]
        public void It_UsesTheSessionIdHeader()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Session-Id"] = "abc-123";

            var key = RateLimitConfig.GetSessionOrIpKey(httpContext);

            Assert.Equal("session:abc-123", key);
        }

        [Fact]
        public void It_FallsBackToTheIpKeyWhenNoSessionIdHeaderExists()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");

            var key = RateLimitConfig.GetSessionOrIpKey(httpContext);

            Assert.Equal("ip:203.0.113.10", key);
        }
    }
}
