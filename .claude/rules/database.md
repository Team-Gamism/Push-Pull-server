---
paths:
  - "PushAndPull/Domain/**/Entity/**/*.cs"
  - "PushAndPull/Domain/**/Repository/**/*.cs"
  - "PushAndPull/Global/Infrastructure/**/*.cs"
  - "PushAndPull/Migrations/**/*.cs"
---

# Database Rules

- Use PostgreSQL with EF Core and Npgsql.
- Use EF Core Fluent API configuration classes under each entity's `Config` folder.
- Keep table, column, index, and schema names explicit in Fluent API using `.HasColumnName(...)`, `.ToTable(...)`, and explicit index names.
- Use snake_case database identifiers.
- Keep domain schemas separate, such as `auth` and `room`.
- Use PostgreSQL `timestamptz` for `DateTimeOffset` values.
- Do not expose `IQueryable` from repositories.
- Do not edit committed migrations to rewrite history; add a new migration for schema changes.
- After entity or configuration changes, create a migration with a descriptive PascalCase name.
- Review generated migrations before committing to ensure no unrelated schema churn is included.
