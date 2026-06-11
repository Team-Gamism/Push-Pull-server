---
paths:
  - "PushAndPull/**/*.cs"
  - "PushAndPull/**/*.json"
---

# Project Conventions

- API routes use the `api/v1/{resource}` pattern.
- Wrap controller responses with `CommonApiResponse` or `CommonApiResponse<T>`.
- Use Korean user-facing response messages to match existing controllers.
- Protected endpoints use `[SessionAuthorize]` and read identity through `ClaimsPrincipalExtensions`.
- Steam session authentication uses the `Session-Id` header; do not add Bearer/JWT behavior unless the project explicitly changes auth strategy.
- Cache keys must be created in `CacheKey`; avoid hardcoded Redis key strings outside that class.
- Add new domain dependencies in the matching `Domain/{Feature}/Config/*ServiceConfig.cs` file.
- Add shared dependencies in `Global/Config`.
