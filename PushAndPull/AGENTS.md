# AGENTS.md — Codex Advisory Reviewer Context

Push & Pull Server: Steam-authenticated game room management API (.NET 9, PostgreSQL, Redis).

## Build & Test Commands

```bash
dotnet build PushAndPull.sln
dotnet test PushAndPull.Test/PushAndPull.Test.csproj
```

## Absolute Prohibitions

- Never commit secrets, API keys, or connection strings to the repo
- Never use `git push --force` or `git commit --no-verify`
- Never bypass `[SessionAuthorize]` on endpoints that modify state

## Review Priority Order

1. **Security** — Steam ticket validation paths, BCrypt usage, Redis session handling
2. **Data integrity** — Room status transitions, atomic player count operations
3. **Type safety** — Nullable reference types, EF Core tracking state bugs
4. **Domain rules** — See below
5. **Performance** — N+1 queries, missing indexes

## Domain Integrity Rules

- All read queries must use `AsNoTracking()` — tracked reads outside write paths are a bug
- Player count increments must use `ExecuteUpdateAsync` (set-based) — `SaveChanges` on a tracked entity is a race condition
- Password hashing must go through `IPasswordHasher` — calling `BCrypt.HashPassword` directly bypasses the workFactor contract
- `[SessionAuthorize]` omission on a state-mutating endpoint requires an explicit comment explaining why
- Room status comparisons must use the `RoomStatus` enum, never raw strings
- Foreign key to `auth.user` must use `DeleteBehavior.Restrict` — cascade deletes on user data are forbidden

## Forbidden Patterns

- Hardcoded Steam API keys, DB connection strings, or Redis URIs in source files
- Raw SQL strings without parameterization (Dapper queries must use anonymous objects)
- `string` literals where `RoomStatus` enum values exist
- Catching `Exception` without re-throwing or specific handling

## Output Format

For each finding:
```
[SEVERITY] File:Line — Description
  → Suggested fix
```
Severity: `CRITICAL` | `WARNING` | `INFO`

Group by severity, highest first.

## What to Ignore

- Code style preferences and naming conventions (those belong to CLAUDE.md)
- General refactoring suggestions unrelated to domain rule violations
- Test coverage opinions unless a critical path has zero coverage
- Framework boilerplate that follows the established pattern in the codebase
