# Push & Pull Server — Claude Code Guide

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
| `Gamism.SDK.Extensions.AspNetCore` | 0.5.0 | — (no Context7 entry) |
| `xunit` | 2.9.2 | `/xunit/xunit.net` |
| `Moq` | 4.20.72 | — (no Context7 entry) |

