---
description: Context7 MCP usage — library ID mapping and query patterns for fetching version-accurate docs
globs:
---

# Context7 MCP Usage

When working with any library listed in the tech stack, use the Context7 MCP to fetch version-accurate official documentation before writing or modifying code.

## Library ID Mapping

| Package | Context7 ID |
|---|---|
| .NET / ASP.NET Core | `/dotnet/docs` |
| `Microsoft.EntityFrameworkCore` | `/dotnet/docs` |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | `/npgsql/efcore.pg` |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | `/stackexchange/stackexchange.redis` |
| `Dapper` | `/dotnet/docs` |
| `xunit` | `/xunit/xunit.net` |

`BCrypt.Net-Next`, `Moq`, and `Gamism.SDK.Extensions.AspNetCore` have no Context7 entry — refer to the source code directly.

## Query Examples

```
# EF Core Fluent API
mcp__context7__query-docs(libraryId: "/dotnet/docs", query: "EF Core IEntityTypeConfiguration fluent API", version: "9.0")

# Npgsql EF Core setup
mcp__context7__query-docs(libraryId: "/npgsql/efcore.pg", query: "UseNpgsql configuration")

# Redis session
mcp__context7__query-docs(libraryId: "/stackexchange/stackexchange.redis", query: "IDistributedCache SetString GetString")
```
