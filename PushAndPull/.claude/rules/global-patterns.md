---
description: Global infrastructure patterns (Steam Auth, Redis Session, CacheKey, Security). Applied when working on Global/** files.
globs: ["PushAndPull/Global/**"]
alwaysApply: false
---

## Global Config Extension Methods

All infrastructure registrations are `IServiceCollection` extension methods in `Global/Config/`:

| Class | Method | Purpose |
|---|---|---|
| `DatabaseConfig` | `AddDatabase` | EF Core + PostgreSQL (Npgsql) |
| `RedisConfig` | `AddRedis` | StackExchange.Redis + IDistributedCache |

## Steam Authentication

Steam ticket validation is handled by `IAuthTicketValidator` in `Global/Auth/`:

- `ValidateAsync(ticket)` — calls `ISteamUserAuth/AuthenticateUserTicket/v1` and returns `AuthTicketValidationResult`
- Returns `steamId` (`ulong`) on success
- Throws `UnauthorizedException` on invalid/expired ticket

## Session Store (Redis)

Sessions are stored in Redis via `ISessionService` / `ISessionStore`:

- Session key: `CacheKey.Session.ById(sessionId)`
- Session contains `SteamId` (`ulong`)
- TTL: 15 days (configurable)

**All Redis keys must use `CacheKey` — no hardcoded strings:**

```csharp
// Correct
CacheKey.Session.ById(sessionId)

// Forbidden
"session:" + sessionId
```

## Security

`[SessionAuthorize]` attribute validates `Session-Id` header on each request:
- Reads session from Redis
- Populates `ClaimsPrincipal` with `SteamId` and `SessionId`

Extension methods for claims extraction:

```csharp
// In controller (after [SessionAuthorize])
ulong steamId = User.GetSteamId();
string sessionId = User.GetSessionId();
```

**SteamId type rule:**
- Entity: `ulong`
- Claims: `long` (stored as long in JWT/claims)
- Always convert when crossing the boundary

## Shared Global Services

Registered via `GlobalServiceConfig` in `Global/Config/`:

- `IPasswordHasher` — BCrypt wrapper
- `IRoomCodeGenerator` — generates unique room codes
- `IAuthTicketValidator` — Steam ticket validator
- `ISessionService` — Redis session management
