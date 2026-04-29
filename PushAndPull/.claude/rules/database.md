---
paths:
  - "PushAndPull/Domain/**/Entity/**/*.cs"
  - "PushAndPull/Domain/**/Repository/**/*.cs"
  - "PushAndPull/Migrations/**/*.cs"
---

# Database Rules

- Use PostgreSQL with EF Core and Npgsql.
- Keep table, column, index, and schema names explicit in Fluent API.
- Use snake_case database identifiers.
- Keep domain schemas separate, such as `auth` and `room`.
- Use `AsNoTracking()` for read-only repository queries.
- Do not expose `IQueryable` from repositories.
- Do not edit committed migrations to rewrite history; add a new migration for schema changes.
