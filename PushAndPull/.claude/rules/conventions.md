---
description: Naming, DTO, EF Core entity configuration, and cache conventions. Applied for .cs files.
globs: ["**/*.cs"]
alwaysApply: false
---

## Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Service Interface | `I{Action}Service` | `ILoginService` |
| Repository Interface | `I{Entity}Repository` | `IUserRepository` |
| Command | `{Action}Command` | `LoginCommand` |
| Result | `{Action}Result` | `LoginResult` |
| Request DTO | `{Action}Request` | `CreateRoomRequest` |
| Response DTO | `{Action}Response` | `CreateRoomResponse` |
| Service impl | `{Action}Service` | `LoginService` |

- Database column names: `snake_case`
- Timestamps: `timestamptz`
- Enums stored as strings: `.HasConversion<string>()`

## DTOs

- Use `record` types only — `Request/` for HTTP input, `Response/` for HTTP output.
- No DataAnnotations on entities — only on Request DTOs if needed.

## Entity Configuration (EF Core)

- Use `IEntityTypeConfiguration<T>` Fluent API — **never DataAnnotations on entities**.
- Place in `Domain/{Name}/Entity/Config/`.
- `AppDbContext.OnModelCreating` calls only `modelBuilder.ApplyConfigurationsFromAssembly(...)`.
- See `/db-migration-guide` skill for full examples.

## Cache (Redis)

All Redis keys must go through `CacheKey` — hardcoded strings are **forbidden**.
- Correct: `CacheKey.Session.ById(sessionId)`
- Forbidden: `"session:" + sessionId`

## Domain DI Registration

Each domain exposes a single `Add{Domain}Services(this IServiceCollection)` extension method in `Config/`.
Call all domain configs from `Program.cs`. **Never skip DI registration after adding a new service.**

## Read Queries

Always use `AsNoTracking()` for queries that do not modify entities.
