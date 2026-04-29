---
paths:
  - "PushAndPull/**/*.cs"
  - "PushAndPull.Test/**/*.cs"
---

# Code Style Rules

- Use C# nullable reference types and keep nullability explicit.
- Prefer constructor injection with `private readonly` fields.
- Use async methods with the `Async` suffix when they perform asynchronous work.
- Accept `CancellationToken ct = default` in service and repository async methods that may call I/O.
- Keep command and result types near the service interface they belong to.
- Name service entry points `ExecuteAsync` for use-case services.
- Do not introduce fully qualified type names inline when an alias or `using` can keep the code readable.
- Keep response DTOs separate from entities; never return EF entities directly from controllers.
