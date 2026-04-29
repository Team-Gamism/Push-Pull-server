---
paths:
  - "PushAndPull/Domain/**/Entity/**/*.cs"
  - "PushAndPull/Global/Infrastructure/**/*.cs"
  - "PushAndPull/Migrations/**/*.cs"
---

# Database And Migration Rules

- Use EF Core Fluent API configuration classes under each entity's `Config` folder.
- Keep database names in snake_case through `.HasColumnName(...)`, `.ToTable(...)`, and explicit index names.
- Use schemas by domain, for example `auth` and `room`.
- Use PostgreSQL `timestamptz` for `DateTimeOffset` values.
- Do not edit committed migration files to change schema history; add a new migration instead.
- After entity or configuration changes, create a migration with a descriptive PascalCase name.
- Review generated migrations before committing to ensure no unrelated schema churn is included.
