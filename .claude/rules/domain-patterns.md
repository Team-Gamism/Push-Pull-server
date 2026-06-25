---
paths:
  - "src/PushAndPull/Domain/**/*.cs"
---

# Domain Pattern Rules

- Keep entity constructors responsible for valid initial state.
- Preserve EF Core compatibility with a protected or private parameterless constructor on entities.
- Keep entity setters private unless mutation is part of the domain model.
- Put domain-specific exceptions under `Domain/{Feature}/Exception`.
- Services should throw domain exceptions for expected business failures rather than returning magic status values.
- Repositories should use `AsNoTracking()` for read-only queries.
- Use EF Core set-based updates such as `ExecuteUpdateAsync` when updating rows without needing tracked entities.
