using Server.Domain.Entity;

namespace Tests.Domain;

// Describe: User
public class UserTests
{
    // Context: When updating a nickname with a valid value
    public class WhenUpdatingNicknameWithAValidValue
    {
        private readonly User _user;

        public WhenUpdatingNicknameWithAValidValue()
        {
            _user = new User(76561198000000001UL, "OriginalName");
        }

        [Fact]
        public void It_ChangesTheNickname()
        {
            _user.UpdateNickname("NewName");

            Assert.Equal("NewName", _user.Nickname);
        }
    }

    // Context: When updating a nickname with an empty string
    public class WhenUpdatingNicknameWithAnEmptyString
    {
        private readonly User _user;

        public WhenUpdatingNicknameWithAnEmptyString()
        {
            _user = new User(76561198000000002UL, "SomePlayer");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void It_ThrowsArgumentException(string invalidNickname)
        {
            Assert.Throws<ArgumentException>(() => _user.UpdateNickname(invalidNickname));
        }
    }

    // Context: When updating the last login time
    public class WhenUpdatingLastLoginTime
    {
        private readonly User _user;

        public WhenUpdatingLastLoginTime()
        {
            _user = new User(76561198000000003UL, "LoginPlayer");
        }

        [Fact]
        public void It_UpdatesLastLoginAt()
        {
            var before = _user.LastLoginAt;

            _user.UpdateLastLogin();

            Assert.True(_user.LastLoginAt >= before);
        }
    }

    // Context: When a user is created
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
