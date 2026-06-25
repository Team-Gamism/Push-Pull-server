using PushAndPull.Domain.Auth.Entity;

namespace PushAndPull.Test.Domain.Auth.Entity;

public class UserTests
{
    public class WhenAUserIsCreatedWithAnInvalidNickname
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void It_ThrowsArgumentException(string invalidNickname)
        {
            Assert.Throws<ArgumentException>(() => new User(76561198000000002UL, invalidNickname));
        }
    }

    public class WhenAUserIsCreated
    {
        [Fact]
        public void It_SetsTheSteamId()
        {
            var steamId = 76561198000000004UL;
            var user = new User(steamId, "Player");

            Assert.Equal(steamId, user.SteamId);
        }

        [Fact]
        public void It_SetsTheNickname()
        {
            var user = new User(76561198000000005UL, "MyNick");

            Assert.Equal("MyNick", user.Nickname);
        }
    }
}
