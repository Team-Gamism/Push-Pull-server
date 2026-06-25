---
paths:
  - "src/PushAndPull/appsettings*.json"
  - "src/deploy/*.yaml"
  - "src/deploy/*.dockerfile"
  - "src/PushAndPull/Global/Config/**/*.cs"
---

# Configuration Rules

- Do not hardcode secrets, database passwords, Redis passwords, Steam secrets, or API keys.
- Keep local-only values in user secrets, environment variables, or ignored local config.
- Runtime configuration should be read through `IConfiguration` and registered in `Global/Config`.
- Keep container runtime assumptions aligned with the project: Linux container, ASP.NET Core port `8080`.
- When changing service dependencies, update both development and production compose files if the runtime contract changes.
