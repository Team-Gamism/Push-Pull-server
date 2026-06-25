---
paths:
  - "src/PushAndPull/**/*.cs"
  - "src/PushAndPull.Test/**/*.cs"
---

# Architecture Rules

- Keep the project as a single ASP.NET Core Web API solution targeting .NET 9.
- Organize feature code under `Domain/{Feature}` and shared infrastructure under `Global`.
- Use the request flow `Controller -> Service -> Repository`.
- Controllers should only translate HTTP input/output and delegate behavior to services.
- Services contain application behavior and depend on interfaces, not concrete repositories or infrastructure classes.
- Repositories own EF Core access and should expose behavior-oriented methods instead of leaking `IQueryable`.
- Register dependencies in feature config extension classes such as `AddAuthServices` and `AddRoomServices`, then call them from `Program.cs`.
