using Moq;
using PushAndPull.Domain.Auth.Entity;
using PushAndPull.Domain.Auth.Service;
using PushAndPull.Global.Infrastructure.Cache;

namespace PushAndPull.Test.Domain.Auth.Service;

public class SessionServiceTests
{
    private const ulong SteamId = 76561198000000001UL;
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(15);

    public class WhenAUserLogsInForTheFirstTime
    {
        private readonly Mock<ICacheStore> _cacheStoreMock = new();
        private readonly SessionService _sut;

        public WhenAUserLogsInForTheFirstTime()
        {
            _cacheStoreMock
                .Setup(c => c.GetAsync<string>(CacheKey.Session.BySteamId(SteamId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            _sut = new SessionService(_cacheStoreMock.Object);
        }

        [Fact]
        public async Task It_StoresTheSessionAndTheReverseIndex()
        {
            var session = await _sut.CreateAsync(SteamId, Ttl);

            _cacheStoreMock.Verify(c => c.SetAsync(
                CacheKey.Session.ById(session.SessionId),
                It.Is<PlayerSession>(s => s.SteamId == SteamId),
                Ttl,
                It.IsAny<CancellationToken>()), Times.Once);

            _cacheStoreMock.Verify(c => c.SetAsync(
                CacheKey.Session.BySteamId(SteamId),
                session.SessionId,
                Ttl,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task It_DoesNotDeleteAnyPreviousSession()
        {
            await _sut.CreateAsync(SteamId, Ttl);

            _cacheStoreMock.Verify(
                c => c.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    public class WhenAUserLogsInAgain
    {
        private readonly Mock<ICacheStore> _cacheStoreMock = new();
        private readonly SessionService _sut;

        private const string PreviousSessionId = "previous-session-id";

        public WhenAUserLogsInAgain()
        {
            _cacheStoreMock
                .Setup(c => c.GetAsync<string>(CacheKey.Session.BySteamId(SteamId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PreviousSessionId);

            _sut = new SessionService(_cacheStoreMock.Object);
        }

        [Fact]
        public async Task It_InvalidatesThePreviousSession()
        {
            await _sut.CreateAsync(SteamId, Ttl);

            _cacheStoreMock.Verify(
                c => c.DeleteAsync(CacheKey.Session.ById(PreviousSessionId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task It_PointsTheReverseIndexToTheNewSession()
        {
            var session = await _sut.CreateAsync(SteamId, Ttl);

            _cacheStoreMock.Verify(c => c.SetAsync(
                CacheKey.Session.BySteamId(SteamId),
                session.SessionId,
                Ttl,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class WhenASessionIsDeleted
    {
        private readonly Mock<ICacheStore> _cacheStoreMock = new();
        private readonly SessionService _sut;

        private readonly PlayerSession _session = new(SteamId, Ttl);

        public WhenASessionIsDeleted()
        {
            _cacheStoreMock
                .Setup(c => c.GetAsync<PlayerSession>(CacheKey.Session.ById(_session.SessionId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_session);

            _cacheStoreMock
                .Setup(c => c.GetAsync<string>(CacheKey.Session.BySteamId(SteamId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_session.SessionId);

            _sut = new SessionService(_cacheStoreMock.Object);
        }

        [Fact]
        public async Task It_DeletesTheSessionAndTheReverseIndex()
        {
            await _sut.DeleteAsync(_session.SessionId);

            _cacheStoreMock.Verify(
                c => c.DeleteAsync(CacheKey.Session.ById(_session.SessionId), It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheStoreMock.Verify(
                c => c.DeleteAsync(CacheKey.Session.BySteamId(SteamId), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    public class WhenADeletedSessionsReverseIndexPointsToANewerSession
    {
        private readonly Mock<ICacheStore> _cacheStoreMock = new();
        private readonly SessionService _sut;

        private readonly PlayerSession _session = new(SteamId, Ttl);

        public WhenADeletedSessionsReverseIndexPointsToANewerSession()
        {
            _cacheStoreMock
                .Setup(c => c.GetAsync<PlayerSession>(CacheKey.Session.ById(_session.SessionId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_session);

            _cacheStoreMock
                .Setup(c => c.GetAsync<string>(CacheKey.Session.BySteamId(SteamId), It.IsAny<CancellationToken>()))
                .ReturnsAsync("newer-session-id");

            _sut = new SessionService(_cacheStoreMock.Object);
        }

        [Fact]
        public async Task It_KeepsTheReverseIndex()
        {
            await _sut.DeleteAsync(_session.SessionId);

            _cacheStoreMock.Verify(
                c => c.DeleteAsync(CacheKey.Session.BySteamId(SteamId), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
