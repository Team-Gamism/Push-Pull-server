---
paths:
  - "PushAndPull/Global/**/*.cs"
  - "PushAndPull/Domain/Auth/**/*.cs"
---

# Global Pattern Rules

- Steam ticket validation belongs behind `IAuthTicketValidator`.
- Session creation and lookup belongs behind `ISessionService`.
- Redis access belongs behind `ICacheStore`; domain services should not use `IDistributedCache` directly.
- Session claims must use `SessionClaim.SessionId` and `SessionClaim.SteamId`.
- Do not duplicate session header parsing outside `SessionAuthorizeAttribute`.
- Keep health checks and SDK wrapper exclusions aligned with `Program.cs` and Gamism SDK settings.
