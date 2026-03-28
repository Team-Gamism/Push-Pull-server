---
description: Code review checklist and automatic verification (.NET/C# - PushAndPull)
---
# Code Review Guide
Analyze changed files and verify the following items.
## Check Changes
1. Run `git diff` or `git diff develop...HEAD` to see changed files
2. Read each file with Read tool for detailed analysis
## Checklist
### DTO
- [ ] DTO is defined as `record` type?
- [ ] Request DTO name has `Request` suffix, Response DTO has `Response` suffix?
- [ ] No unnecessary property setters? (record constructor params only)

### C# Style
- [ ] Using `readonly` fields for injected dependencies?
- [ ] Using constructor injection?
- [ ] Handling nullable reference types properly? (`?`, null checks, guard clauses)
- [ ] Async methods have `Async` suffix and return `Task` / `Task<T>`?
- [ ] `CancellationToken ct` passed through to repository/async calls where appropriate?

### EF Core / Database
- [ ] Using `AsNoTracking()` for read-only queries?
- [ ] No N+1 problem? (Using `.Include()` / `.ThenInclude()` where needed?)
- [ ] Using `ExecuteUpdateAsync()` for bulk updates instead of fetch-then-save?
- [ ] Using `ExecuteDeleteAsync()` for bulk deletes instead of fetch-then-remove?
- [ ] `SaveChangesAsync()` called only when not using `ExecuteUpdateAsync` / `ExecuteDeleteAsync`?
- [ ] Using transactions explicitly where multiple writes are involved?

### Test
- [ ] Test code written?
- [ ] Test class uses nested class per scenario (e.g. `WhenANewUserLogsIn`)?
- [ ] Following Arrange(constructor)-Act-Assert pattern?
- [ ] Using `Moq` `Setup` in constructor, `Verify` in each test?
- [ ] Verifying both positive behavior and negative behavior (e.g. `Times.Never`)?

### Commit
- [ ] Following commit message convention?
- [ ] Using domain name for scope? (module names allowed only for cross-cutting concerns)
- [ ] Commits split into logical units?

### Other
- [ ] No excessive comments?
- [ ] No hardcoded secrets? (Use `IConfiguration` / environment variables / Secret Manager)
- [ ] No sensitive information in logs?

## Report Format
For each item:
- ✓ Pass
- ⚠ Warning (recommendation)
- ✗ Error (needs fix)
  Final summary:
- Total {n} items verified
- {p} passed, {w} warnings, {e} errors