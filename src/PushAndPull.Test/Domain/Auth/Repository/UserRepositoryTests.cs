using Microsoft.EntityFrameworkCore;
using PushAndPull.Domain.Auth.Entity;
using PushAndPull.Domain.Auth.Repository;
using PushAndPull.Test.Support;

namespace PushAndPull.Test.Domain.Auth.Repository;

public class UserRepositoryTests
{
    [Collection("Postgres")]
    public class WhenGettingBySteamId(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_ReturnsTheMatchingUser()
        {
            Db.Users.Add(new User(42UL, "alice"));
            await Db.SaveChangesAsync();

            var user = await new UserRepository(Db).GetBySteamIdAsync(42UL);

            Assert.Equal("alice", user?.Nickname);
        }

        [Fact]
        public async Task It_ReturnsNullWhenNotFound()
        {
            var user = await new UserRepository(Db).GetBySteamIdAsync(999UL);

            Assert.Null(user);
        }
    }

    [Collection("Postgres")]
    public class WhenCreatingAUser(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_PersistsTheUser()
        {
            await new UserRepository(Db).CreateAsync(new User(7UL, "bob"));

            var stored = await Db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.SteamId == 7UL);
            Assert.Equal("bob", stored?.Nickname);
        }
    }

    [Collection("Postgres")]
    public class WhenUpdatingAUser(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_UpdatesNicknameAndLastLogin()
        {
            Db.Users.Add(new User(5UL, "old-name"));
            await Db.SaveChangesAsync();
            var loginAt = new DateTimeOffset(2031, 2, 3, 4, 5, 6, TimeSpan.Zero);

            await new UserRepository(Db).UpdateAsync(5UL, "new-name", loginAt);

            var user = await Db.Users.AsNoTracking().FirstAsync(u => u.SteamId == 5UL);
            Assert.Equal("new-name", user.Nickname);
            Assert.Equal(loginAt, user.LastLoginAt);
        }
    }
}
