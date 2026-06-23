---
name: verify
description: Verify PushAndPull C# changes by building and testing the solution. Use after any C# code change (build), after behavior or test changes (build + test), and check migration intent when EF Core entities or configurations changed. Reports honestly what was and was not run.
---

Run this after changing C# code in the PushAndPull solution to confirm the change compiles and behaves as intended. All commands run from the repository root.

## Step 1 — Build (after any C# change)

```bash
dotnet build PushAndPull/PushAndPull.sln --nologo
```

Fix every build error before moving on. Do not report work as complete while the build is broken.

## Step 2 — Test (after behavior or test changes)

```bash
dotnet test PushAndPull/PushAndPull.sln --nologo
```

Required whenever you changed service behavior, entity invariants, auth/session logic, repository queries, or added/updated tests.

## Step 3 — Migration intent check (only if EF Core entities/configs changed)

If you touched entities or `IEntityTypeConfiguration` files, confirm generated migrations are **either intentionally added or intentionally absent**:

- A schema change with no new migration is a problem — generate one (see the `db-migrate` skill).
- A new migration with unexplained churn is a problem — investigate before committing.

## Reporting rules

- State plainly what was run and the result.
- If build or test was skipped, say so and why — never imply verification that did not happen.
- If tests fail, surface the failing output; do not claim success.
