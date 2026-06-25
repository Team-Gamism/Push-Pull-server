---
paths:
  - "src/PushAndPull/**/*.cs"
  - "src/PushAndPull/appsettings*.json"
  - "src/deploy/*.yaml"
---

# Security Rules

- Never commit secrets or credentials in appsettings, compose files, or source code.
- Steam auth ticket validation must stay behind `IAuthTicketValidator`.
- Session authentication uses the `Session-Id` header and `SessionAuthorizeAttribute`.
- Do not introduce JWT/Bearer auth unless the project explicitly changes auth strategy.
- Never store raw private-room passwords; store only hashes created by `IPasswordHasher`.
- Keep session data in Redis through `ISessionService` and `ICacheStore`.
- Apply rate limiting to login and other abuse-prone endpoints.
