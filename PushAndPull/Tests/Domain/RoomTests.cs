using Server.Domain.Entity;

namespace Tests.Domain;

// Describe: Room
public class RoomTests
{
    // Context: When a player joins an active room
    public class WhenAPlayerJoinsAnActiveRoom
    {
        private readonly Room _room;

        public WhenAPlayerJoinsAnActiveRoom()
        {
            _room = new Room("ROOM01", "Test Room", 111UL, 76561198000000001UL, false, null);
        }

        [Fact]
        public void It_IncreasesCurrentPlayerCount()
        {
            var before = _room.CurrentPlayers;

            _room.Join();

            Assert.Equal(before + 1, _room.CurrentPlayers);
        }
    }

    // Context: When a player tries to join a full room
    public class WhenAPlayerTriesToJoinAFullRoom
    {
        private readonly Room _room;

        public WhenAPlayerTriesToJoinAFullRoom()
        {
            _room = new Room("ROOM02", "Full Room", 222UL, 76561198000000001UL, false, null);
            _room.Join(); // MaxPlayers = 2, now full
        }

        [Fact]
        public void It_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _room.Join());

            Assert.Equal("FULL_ROOM", ex.Message);
        }
    }

    // Context: When a room is marked as deleting
    public class WhenARoomIsMarkedAsDeleting
    {
        private readonly Room _room;
        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

        public WhenARoomIsMarkedAsDeleting()
        {
            _room = new Room("ROOM03", "Deleting Room", 333UL, 76561198000000001UL, false, null);
        }

        [Fact]
        public void It_ChangesStatusToDeleting()
        {
            _room.MarkDeleting(_ttl);

            Assert.Equal(RoomStatus.Deleting, _room.Status);
        }

        [Fact]
        public void It_SetsExpiresAt()
        {
            var before = DateTimeOffset.UtcNow;

            _room.MarkDeleting(_ttl);

            Assert.NotNull(_room.ExpiresAt);
            Assert.True(_room.ExpiresAt >= before.Add(_ttl));
        }
    }

    // Context: When a room is closed
    public class WhenARoomIsClosed
    {
        private readonly Room _room;

        public WhenARoomIsClosed()
        {
            _room = new Room("ROOM04", "Closing Room", 444UL, 76561198000000001UL, false, null);
        }

        [Fact]
        public void It_ChangesStatusToClosed()
        {
            _room.Close();

            Assert.Equal(RoomStatus.Closed, _room.Status);
        }

        [Fact]
        public void It_SetsExpiresAtToNow()
        {
            var before = DateTimeOffset.UtcNow;

            _room.Close();

            Assert.NotNull(_room.ExpiresAt);
            Assert.True(_room.ExpiresAt >= before);
        }
    }

    // Context: When a room is created
    public class WhenARoomIsCreated
    {
        [Fact]
        public void It_StartsWithOnePlayer()
        {
            var room = new Room("ROOM05", "New Room", 555UL, 76561198000000001UL, false, null);

            Assert.Equal(1, room.CurrentPlayers);
        }

        [Fact]
        public void It_StartsWithActiveStatus()
        {
            var room = new Room("ROOM06", "Active Room", 666UL, 76561198000000001UL, false, null);

            Assert.Equal(RoomStatus.Active, room.Status);
        }
    }
}
