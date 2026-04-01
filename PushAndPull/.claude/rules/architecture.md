---
description: Overall architecture, directory structure, and layering rules. Always applied.
globs:
alwaysApply: true
---

## Directory Structure

```
PushAndPull/
├── Domain/
│   ├── {DomainName}/
│   │   ├── Config/         # DI registration extension methods
│   │   ├── Controller/     # ASP.NET Core controllers
│   │   ├── Dto/
│   │   │   ├── Request/
│   │   │   └── Response/
│   │   ├── Entity/
│   │   │   └── Config/     # EF Core Fluent API configurations
│   │   ├── Exception/      # Domain-specific exceptions
│   │   ├── Repository/
│   │   │   └── Interface/
│   │   └── Service/
│   │       └── Interface/  # Service interface + Command/Result records
└── Global/
    ├── Auth/               # Steam ticket validation (IAuthTicketValidator)
    ├── Cache/              # Redis cache (ICacheStore, CacheKey)
    ├── Config/             # Infrastructure registrations (DB, Redis)
    ├── Infrastructure/     # AppDbContext
    ├── Security/           # [SessionAuthorize], ClaimsPrincipalExtensions
    └── Service/            # Shared utilities (IPasswordHasher, IRoomCodeGenerator)
```

## Layering Rules

- **Controllers** depend on service interfaces only — never concrete services or repositories.
- **Services** depend on repository interfaces and global service interfaces only.
- **Repositories** are the only layer that touches `AppDbContext`.
- Interfaces (repository + service) are co-located within their domain folder.

Forbidden:
```
Controller → Repository              ❌
Controller → concrete class          ❌
Service    → concrete Repository     ❌
Entity     → Service / Repository    ❌
```

## Adding a New Domain

1. Define entity in `Domain/{Name}/Entity/`
2. Add EF Fluent config in `Domain/{Name}/Entity/Config/`
3. Register `DbSet<T>` in `AppDbContext`
4. Define DTOs (records) in `Domain/{Name}/Dto/Request/` and `Dto/Response/`
5. Define repository interface in `Domain/{Name}/Repository/Interface/`
6. Define service interface(s) + Command/Result records in `Domain/{Name}/Service/Interface/`
7. Implement repository and service
8. Create `Domain/{Name}/Config/{Name}ServiceConfig.cs` with DI extension method
9. Call the extension method from `Program.cs`
