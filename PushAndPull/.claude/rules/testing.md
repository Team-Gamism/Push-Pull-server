---
description: xUnit + Moq test conventions. Applied when working on PushAndPull.Test/** files.
globs: ["PushAndPull.Test/**"]
alwaysApply: false
---

## Test Project Structure

```
PushAndPull.Test/
├── Domain/
│   └── {DomainName}/       # Entity unit tests
└── Service/
    └── {DomainName}/       # Service unit tests
```

## Conventions

- Test class name: `{ServiceName}Tests` — e.g., `LoginServiceTests`
- Nested class per scenario (English): `WhenANewUserLogsIn`, `WhenTheRoomIsFull`
- Test method name: `It_{ExpectedResult}` — e.g., `It_CreatesANewUser`, `It_ThrowsNotFoundException`
- Use `Moq` for mocking — mock repository interfaces, not `AppDbContext` directly.
- Constructor for mock setup; each test method for assertion + verification.

```csharp
public class LoginServiceTests
{
    public class WhenANewUserLogsInForTheFirstTime
    {
        private readonly Mock<IAuthTicketValidator> _validatorMock = new();
        private readonly Mock<ISessionService> _sessionServiceMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly LoginService _sut;

        private const string Ticket = "valid-ticket";
        private const ulong SteamId = 76561198000000001UL;

        public WhenANewUserLogsInForTheFirstTime()
        {
            _validatorMock
                .Setup(v => v.ValidateAsync(Ticket))
                .ReturnsAsync(new AuthTicketValidationResult(SteamId, SteamId, false, false));

            _userRepositoryMock
                .Setup(r => r.GetBySteamIdAsync(SteamId, CancellationToken.None))
                .ReturnsAsync((User?)null);

            _sut = new LoginService(_validatorMock.Object, _sessionServiceMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task It_CreatesANewUser()
        {
            await _sut.ExecuteAsync(new LoginCommand(Ticket, "Player"));

            _userRepositoryMock.Verify(r => r.CreateAsync(
                It.Is<User>(u => u.SteamId == SteamId),
                CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task It_DoesNotCallUpdateUser()
        {
            await _sut.ExecuteAsync(new LoginCommand(Ticket, "Player"));

            _userRepositoryMock.Verify(r => r.UpdateAsync(
                It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<DateTime>(), CancellationToken.None), Times.Never);
        }
    }
}
```

## Arrange / Act / Assert

- Separate Arrange (constructor), Act, Assert with blank lines.
- Use `xunit` built-in `Assert` — no FluentAssertions in this project.
- Verify both positive behavior (`Times.Once`) and negative behavior (`Times.Never`).
