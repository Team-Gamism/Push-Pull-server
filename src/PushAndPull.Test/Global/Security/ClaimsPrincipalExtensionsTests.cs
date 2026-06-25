using System.Security.Claims;
using PushAndPull.Global.Security;

namespace PushAndPull.Test.Global.Security;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal PrincipalWith(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "Session"));

    public class WhenGettingTheSessionId
    {
        [Fact]
        public void It_ReturnsTheSessionIdClaimValue()
        {
            var principal = PrincipalWith(new Claim(SessionClaim.SessionId, "session-abc"));

            Assert.Equal("session-abc", principal.GetSessionId());
        }

        [Fact]
        public void It_ThrowsWhenTheSessionIdClaimIsMissing()
        {
            var principal = PrincipalWith();

            var ex = Assert.Throws<UnauthorizedAccessException>(() => principal.GetSessionId());
            Assert.Equal("SESSION_ID_MISSING", ex.Message);
        }
    }

    public class WhenGettingTheSteamId
    {
        [Fact]
        public void It_ReturnsTheParsedSteamId()
        {
            var principal = PrincipalWith(new Claim(SessionClaim.SteamId, "76561198000000001"));

            Assert.Equal(76561198000000001UL, principal.GetSteamId());
        }

        [Fact]
        public void It_ThrowsWhenTheSteamIdClaimIsMissing()
        {
            var principal = PrincipalWith();

            var ex = Assert.Throws<UnauthorizedAccessException>(() => principal.GetSteamId());
            Assert.Equal("STEAM_ID_MISSING", ex.Message);
        }

        [Fact]
        public void It_ThrowsWhenTheSteamIdClaimIsNotANumber()
        {
            var principal = PrincipalWith(new Claim(SessionClaim.SteamId, "not-a-number"));

            var ex = Assert.Throws<UnauthorizedAccessException>(() => principal.GetSteamId());
            Assert.Equal("INVALID_STEAM_ID", ex.Message);
        }
    }
}
