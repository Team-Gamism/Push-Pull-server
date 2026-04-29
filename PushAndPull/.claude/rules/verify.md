---
paths:
  - "PushAndPull/**/*.cs"
  - "PushAndPull.Test/**/*.cs"
  - "*.sln"
  - "**/*.csproj"
---

# Verification Rules

- After any C# code change, run `dotnet build PushAndPull.sln`.
- After behavior or test changes, run `dotnet test PushAndPull.sln`.
- If EF Core entity or configuration files changed, verify generated migrations are either intentionally added or intentionally absent.
- Do not report work as complete when build or tests were skipped; state what was not run and why.
