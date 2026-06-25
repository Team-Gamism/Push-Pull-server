using Microsoft.EntityFrameworkCore;
using PushAndPull.Domain.Auth.Entity;
using PushAndPull.Domain.Room.Entity;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository;
using PushAndPull.Global.Infrastructure;
using PushAndPull.Test.Support;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace PushAndPull.Test.Domain.Room.Repository;

public class RoomRepositoryTests
{
    private static readonly DateTimeOffset BaseTime = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static DateTimeOffset At(int hours) => BaseTime.AddHours(hours);

    private static void SeedUser(AppDbContext db, ulong steamId)
    {
        if (!db.Users.Any(u => u.SteamId == steamId))
        {
            db.Users.Add(new User(steamId, $"user-{steamId}"));
            db.SaveChanges();
        }
    }

    private static void SeedRoom(AppDbContext db, ulong hostSteamId, string code, bool isPrivate = false)
    {
        SeedUser(db, hostSteamId);
        db.Rooms.Add(new EntityRoom(code, $"room-{code}", 0UL, hostSteamId, isPrivate, null));
        db.SaveChanges();
    }

    private static Task<EntityRoom> ReloadAsync(AppDbContext db, string code)
        => db.Rooms.AsNoTracking().FirstAsync(r => r.RoomCode == code);

    private static Task SetCreatedAt(AppDbContext db, string code, DateTimeOffset value)
        => db.Rooms.Where(r => r.RoomCode == code)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.CreatedAt, value));

    private static Task SetGuestHeartbeat(AppDbContext db, string code, DateTimeOffset value)
        => db.Rooms.Where(r => r.RoomCode == code)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.GuestLastHeartbeatAt, value));

    [Collection("Postgres")]
    public class WhenGettingByRoomCode(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_ReturnsTheMatchingRoom()
        {
            SeedRoom(Db, 1UL, "ABC123");

            var room = await new RoomRepository(Db).GetAsync("ABC123");

            Assert.Equal("ABC123", room?.RoomCode);
        }

        [Fact]
        public async Task It_ReturnsNullWhenNotFound()
        {
            var room = await new RoomRepository(Db).GetAsync("MISSING");

            Assert.Null(room);
        }
    }

    [Collection("Postgres")]
    public class WhenListingRooms(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_ReturnsOnlyActivePublicRooms()
        {
            SeedRoom(Db, 1UL, "PUBLIC");
            SeedRoom(Db, 2UL, "PRIVATE", isPrivate: true);
            SeedRoom(Db, 3UL, "CLOSED");
            await new RoomRepository(Db).CloseAsync("CLOSED");

            var rooms = await new RoomRepository(Db).GetAllAsync(0, 10);

            Assert.Equal(new[] { "PUBLIC" }, rooms.Select(r => r.RoomCode));
        }

        [Fact]
        public async Task It_OrdersByCreatedAtDescending()
        {
            SeedRoom(Db, 1UL, "AAA");
            SeedRoom(Db, 2UL, "BBB");
            SeedRoom(Db, 3UL, "CCC");
            await SetCreatedAt(Db, "AAA", At(1));
            await SetCreatedAt(Db, "BBB", At(3));
            await SetCreatedAt(Db, "CCC", At(2));

            var rooms = await new RoomRepository(Db).GetAllAsync(0, 10);

            Assert.Equal(new[] { "BBB", "CCC", "AAA" }, rooms.Select(r => r.RoomCode));
        }

        [Fact]
        public async Task It_AppliesSkipAndTake()
        {
            SeedRoom(Db, 1UL, "AAA");
            SeedRoom(Db, 2UL, "BBB");
            SeedRoom(Db, 3UL, "CCC");
            await SetCreatedAt(Db, "AAA", At(3));
            await SetCreatedAt(Db, "BBB", At(2));
            await SetCreatedAt(Db, "CCC", At(1));

            var rooms = await new RoomRepository(Db).GetAllAsync(1, 1);

            Assert.Equal(new[] { "BBB" }, rooms.Select(r => r.RoomCode));
        }
    }

    [Collection("Postgres")]
    public class WhenJoiningARoom(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_AddsGuestAndIncrementsPlayerCount()
        {
            SeedRoom(Db, 1UL, "JOIN1");

            var joined = await new RoomRepository(Db).TryJoinAsync("JOIN1", 99UL);

            Assert.True(joined);
            var room = await ReloadAsync(Db, "JOIN1");
            Assert.Equal(99UL, room.GuestSteamId);
            Assert.Equal(2, room.CurrentPlayers);
        }

        [Fact]
        public async Task It_ReturnsFalseWhenRoomAlreadyHasGuest()
        {
            SeedRoom(Db, 1UL, "JOIN1");
            var repo = new RoomRepository(Db);
            await repo.TryJoinAsync("JOIN1", 99UL);

            var joined = await repo.TryJoinAsync("JOIN1", 100UL);

            Assert.False(joined);
        }

        [Fact]
        public async Task It_ReturnsFalseWhenHostJoinsOwnRoom()
        {
            SeedRoom(Db, 1UL, "JOIN1");

            var joined = await new RoomRepository(Db).TryJoinAsync("JOIN1", 1UL);

            Assert.False(joined);
        }

        [Fact]
        public async Task It_ReturnsFalseWhenRoomIsNotActive()
        {
            SeedRoom(Db, 1UL, "JOIN1");
            await new RoomRepository(Db).CloseAsync("JOIN1");

            var joined = await new RoomRepository(Db).TryJoinAsync("JOIN1", 99UL);

            Assert.False(joined);
        }
    }

    [Collection("Postgres")]
    public class WhenRemovingAGuest(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_RemovesGuestAndDecrementsPlayerCount()
        {
            SeedRoom(Db, 1UL, "RM1");
            var repo = new RoomRepository(Db);
            await repo.TryJoinAsync("RM1", 99UL);

            var removed = await repo.TryRemoveGuestAsync("RM1", 99UL);

            Assert.True(removed);
            var room = await ReloadAsync(Db, "RM1");
            Assert.Null(room.GuestSteamId);
            Assert.Equal(1, room.CurrentPlayers);
        }

        [Fact]
        public async Task It_ReturnsFalseWhenThereIsNoGuestToRemove()
        {
            SeedRoom(Db, 1UL, "RM1");

            var removed = await new RoomRepository(Db).TryRemoveGuestAsync("RM1", 99UL);

            Assert.False(removed);
        }
    }

    [Collection("Postgres")]
    public class WhenClosingARoom(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_MarksTheRoomAsClosed()
        {
            SeedRoom(Db, 1UL, "CL1");

            await new RoomRepository(Db).CloseAsync("CL1");

            var room = await ReloadAsync(Db, "CL1");
            Assert.Equal(RoomStatus.Closed, room.Status);
        }
    }

    [Collection("Postgres")]
    public class WhenCreatingARoomWithADuplicateCode(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_ThrowsDuplicateRoomCodeException()
        {
            SeedRoom(Db, 1UL, "DUP");
            SeedUser(Db, 2UL);
            var duplicate = new EntityRoom("DUP", "room-dup-2", 0UL, 2UL, false, null);

            await Assert.ThrowsAsync<DuplicateRoomCodeException>(
                () => new RoomRepository(Db).CreateAsync(duplicate));
        }
    }

    [Collection("Postgres")]
    public class WhenUpdatingHeartbeat(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_UpdatesTheHostHeartbeat()
        {
            SeedRoom(Db, 1UL, "HB");

            var ok = await new RoomRepository(Db).UpdateHeartbeatAsync("HB", 1UL, At(5));

            Assert.True(ok);
            var room = await ReloadAsync(Db, "HB");
            Assert.Equal(At(5), room.LastHeartbeatAt);
        }

        [Fact]
        public async Task It_UpdatesTheGuestHeartbeat()
        {
            SeedRoom(Db, 1UL, "HB");
            var repo = new RoomRepository(Db);
            await repo.TryJoinAsync("HB", 99UL);

            await repo.UpdateHeartbeatAsync("HB", 99UL, At(5));

            var room = await ReloadAsync(Db, "HB");
            Assert.Equal(At(5), room.GuestLastHeartbeatAt);
        }

        [Fact]
        public async Task It_ReturnsFalseForNonParticipants()
        {
            SeedRoom(Db, 1UL, "HB");

            var ok = await new RoomRepository(Db).UpdateHeartbeatAsync("HB", 12345UL, At(5));

            Assert.False(ok);
        }
    }

    [Collection("Postgres")]
    public class WhenClosingStaleRooms(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_ClosesRoomsWithHeartbeatBeforeCutoff()
        {
            SeedRoom(Db, 1UL, "OLD");
            await SetCreatedAt(Db, "OLD", At(1));

            var count = await new RoomRepository(Db).CloseStaleRoomsAsync(At(5));

            Assert.Equal(1, count);
            var room = await ReloadAsync(Db, "OLD");
            Assert.Equal(RoomStatus.Closed, room.Status);
        }

        [Fact]
        public async Task It_DoesNotCloseFreshRooms()
        {
            SeedRoom(Db, 1UL, "FRESH");
            await SetCreatedAt(Db, "FRESH", At(10));

            var count = await new RoomRepository(Db).CloseStaleRoomsAsync(At(5));

            Assert.Equal(0, count);
        }
    }

    [Collection("Postgres")]
    public class WhenFreeingStaleGuests(PostgresFixture fixture) : RepositoryTestBase(fixture)
    {
        [Fact]
        public async Task It_FreesGuestsIdleBeforeCutoff()
        {
            SeedRoom(Db, 1UL, "G");
            var repo = new RoomRepository(Db);
            await repo.TryJoinAsync("G", 99UL);
            await SetGuestHeartbeat(Db, "G", At(1));

            var count = await repo.FreeStaleGuestsAsync(At(5));

            Assert.Equal(1, count);
            var room = await ReloadAsync(Db, "G");
            Assert.Null(room.GuestSteamId);
            Assert.Equal(1, room.CurrentPlayers);
        }
    }
}
