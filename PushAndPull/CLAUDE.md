# Push & Pull Server — Claude Code Guide

**Always respond in Korean.**

**All skill files, command files, hooks, and sub-agent configuration files must be written in English.**

## Solution Overview

.NET 9 single-solution backend:

| Project | Type | Role |
|---|---|---|
| `PushAndPull` | ASP.NET Core Web API | Steam auth, room management, session handling |
| `PushAndPull.Test` | xUnit Test Project | Unit tests for services and entities |

| Item | Value |
|---|---|
| Runtime | Linux container, port 8080 |
| Database | PostgreSQL (EF Core 9 + Npgsql) |
| Cache | Redis (session store) |
| Auth | Steam ticket (`Session-Id` header, no Bearer) |

## Tech Stack

| Package | Version | Context7 ID |
|---|---|---|
| .NET / ASP.NET Core | 9.0 | `/dotnet/docs` |
| `Microsoft.EntityFrameworkCore` | 9.0.12 | `/dotnet/docs` |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | `/npgsql/efcore.pg` |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 9.0.3 | `/stackexchange/stackexchange.redis` |
| `Dapper` | 2.1.66 | `/dotnet/docs` |
| `BCrypt.Net-Next` | 4.0.3 | — (no Context7 entry) |
| `Gamism.SDK.Extensions.AspNetCore` | 0.2.8 | — (no Context7 entry) |
| `xunit` | 2.9.2 | `/xunit/xunit.net` |
| `Moq` | 4.20.72 | — (no Context7 entry) |

## Context7 Usage

When working with any library listed above, use the Context7 MCP to fetch version-accurate official documentation before writing or modifying code.

```
# Example: EF Core Fluent API
mcp__context7__query-docs(libraryId: "/dotnet/docs", query: "EF Core IEntityTypeConfiguration fluent API", version: "9.0")

# Example: Npgsql EF Core setup
mcp__context7__query-docs(libraryId: "/npgsql/efcore.pg", query: "UseNpgsql configuration")

# Example: Redis session
mcp__context7__query-docs(libraryId: "/stackexchange/stackexchange.redis", query: "IDistributedCache SetString GetString")
```

`BCrypt.Net-Next`, `Moq`, and `Gamism.SDK.Extensions.AspNetCore` have no Context7 entry — refer to the source code directly.

## Reference Docs

- `.claude/rules/architecture.md` — directory structure and layering rules (Controllers → Services → Repositories)
- `.claude/rules/code-style.md` — C# naming conventions, entity/DTO/async/Command-Result patterns
- `.claude/rules/conventions.md` — DB naming, EF Core Fluent API config, CacheKey usage, DI registration
- `.claude/rules/db-migration.md` — migration workflow, naming convention, entity modification patterns, rollback strategy
- `.claude/rules/domain-patterns.md` — service, repository, and controller implementation patterns
- `.claude/rules/global-patterns.md` — Steam auth, Redis session store, CacheKey, SessionAuthorize
- `.claude/rules/testing.md` — test project structure, naming conventions, Moq patterns
- `.claude/rules/flows.md` — Mermaid sequence diagrams for each API endpoint
- `.claude/rules/verify.md` — build-and-verify workflow (auto build + test after every C# code change)
- `.claude/rules/ask-user.md` — when to use AskUserQuestion (always applied)
- `.claude/rules/test-fixer-trigger.md` — when to proactively offer the test-fixer agent (always applied)
