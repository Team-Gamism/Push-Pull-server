# CLAUDE.md — Push & Pull Server

**Always respond in Korean.**

**All skill files, command files, hooks, and sub-agent configuration files must be written in English.**

---

# Project Overview

Steam-based multiplayer matchmaking / lobby server.

Players authenticate via **Steam tickets**, create or join rooms, and exchange **Steam Lobby IDs**. Actual gameplay runs over **Steam P2P** — this server only handles session auth, room management, and discovery.

| Item | Value |
|------|-------|
| Framework | net9.0 |
| Runtime | Linux container, port 8080 |
| Database | PostgreSQL (EF Core 9) |
| Cache | Redis (session store) |
| Auth | Steam ticket (`Session-Id` header, no Bearer) |

---

# Quick Start (Read This First)

If you're new to this project, read in this order:

1. Architecture
2. Layer Responsibilities
3. Common Mistakes
4. Coding Conventions

Key rules:

- Controller → Service Interface only
- Service → Repository Interface only
- No direct Entity field mutation — use domain methods
- Always `AsNoTracking()` for read queries
- Always `CacheKey` for Redis keys — no hardcoded strings
- EF mapping in `IEntityTypeConfiguration<T>` only — no DataAnnotations
- Register new services in `Domain/{Domain}/Config/` after adding them

---

# Commands

```bash
# Build
dotnet build PushAndPull/PushAndPull.csproj

# Run locally
dotnet run --project PushAndPull/PushAndPull.csproj

# Add EF migration
dotnet ef migrations add <MigrationName> --project PushAndPull

# Apply migration
dotnet ef database update --project PushAndPull

# Docker
docker build -f PushAndPull/Dockerfile -t pushandpull-server .
docker run -p 8080:8080 pushandpull-server
```

---

# Architecture

Domain-centric layered architecture. Each domain (`Auth`, `Room`) is self-contained. Cross-cutting concerns live under `Global/`.

## Dependency Rules

```
Controller  →  Service Interface
Service     →  Repository Interface, Global Service Interface
Repository  →  AppDbContext
```

Forbidden:

```
Controller → Repository              ❌
Controller → concrete class          ❌
Service    → concrete Repository     ❌
Entity     → Service / Repository    ❌
```

## Key Paths

| Layer | Path |
|-------|------|
| Controller | `Domain/{Domain}/Controller/` |
| Service Interface + Command/Result | `Domain/{Domain}/Service/Interface/` |
| Service | `Domain/{Domain}/Service/` |
| Repository Interface | `Domain/{Domain}/Repository/Interface/` |
| Repository | `Domain/{Domain}/Repository/` |
| Entity | `Domain/{Domain}/Entity/` |
| Entity EF Config | `Domain/{Domain}/Entity/Config/` |
| DTO (Request/Response) | `Domain/{Domain}/Dto/Request/`, `Dto/Response/` |
| Exception | `Domain/{Domain}/Exception/` |
| DI Registration | `Domain/{Domain}/Config/` |

## Global/

| Folder | Role |
|--------|------|
| `Auth/` | Steam ticket validation (`IAuthTicketValidator`) |
| `Cache/` | Redis cache (`ICacheStore`, `CacheKey`) |
| `Config/` | Global DI, DB/Redis config |
| `Infrastructure/` | `AppDbContext` |
| `Security/` | `[SessionAuthorize]`, `ClaimsPrincipalExtensions` |
| `Service/` | Shared utilities (`IPasswordHasher`, `IRoomCodeGenerator`) |

---

# Layer Responsibilities

### Controller
- Maps `Request` → `Command`, calls service, maps `Result` → `Response`
- Applies `[SessionAuthorize]` on authenticated endpoints
- Depends **only on Service Interfaces**

### Service
- Implements business logic via `ExecuteAsync(...)`
- `Command` (input) and `Result` (output) records defined in the same Interface file
- Depends only on Repository/Global Interfaces

### Repository
- Abstracts data access (EF Core or Dapper)
- **Read queries must use `AsNoTracking()`**

### Entity
- Pure domain model, no external dependencies
- State changes only through methods (`room.Join()`, `user.UpdateNickname()`)
- Default constructor: `protected`/`private`; public constructor accepts required fields only

### DTO
- `record` type only
- `Request/` for HTTP input, `Response/` for HTTP output

---

# Coding Conventions

## Naming

| Type | Convention | Example |
|------|-----------|---------|
| Service Interface | `I{Action}Service` | `ILoginService` |
| Repository Interface | `I{Entity}Repository` | `IUserRepository` |
| Command | `{Action}Command` | `LoginCommand` |
| Result | `{Action}Result` | `LoginResult` |
| Request DTO | `{Action}Request` | `LoginRequest` |
| Response DTO | `{Action}Response` | `LoginResponse` |
| Service impl | `{Action}Service` | `LoginService` |

## General Rules

- DTOs: `record` type
- Entities / Services: `class`
- Constructor injection only
- Nullable reference types: enabled
- Implicit usings: enabled

---

# Database Rules

- Column naming: `snake_case`
- Timestamps: `timestamptz`
- Enums: `.HasConversion<string>()`
- Read queries: always `AsNoTracking()`

**EF mapping must use `IEntityTypeConfiguration<T>` per entity** (`Domain/{Domain}/Entity/Config/`). DataAnnotations are forbidden.

`AppDbContext.OnModelCreating` contains only:

```csharp
base.OnModelCreating(modelBuilder);
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
```

---

# Cache Rules

Redis is used as a **session store**. All keys must go through `CacheKey` — hardcoded strings are forbidden.

```csharp
// Correct
CacheKey.Session.ById(sessionId)

// Forbidden
"session:" + sessionId
```

---

# Authentication

- Steam: `ISteamUserAuth/AuthenticateUserTicket/v1`
- Session via header: `Session-Id: <id>` (no Bearer tokens)
- SteamId: `ulong` in Entity, `long` in Claims

---

# API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/v1/auth/login` | No | Steam ticket login |
| POST | `/api/v1/auth/logout` | Session | Logout |
| POST | `/api/v1/room` | Session | Create room |
| GET | `/api/v1/room/all` | No | List active rooms |
| GET | `/api/v1/room/{roomCode}` | No | Get room info |
| POST | `/api/v1/room/{roomCode}/join` | Session | Join room |

---

# Commit Message Style

```
{type}: {Korean description}
```

| Type | When |
|------|------|
| `feat` | New feature |
| `fix` | Bug fix, missing config/DI |
| `update` | Modification to existing code |
| `docs` | Documentation changes |
| `merge` | Branch merge |
| `release` | Release (`release/x.x.x`) |

Rules: Korean description, imperative, no trailing punctuation.

```
feat: 방 생성 API 추가
fix: 세션 DI 누락 수정
modify: Room 엔터티 수정
```

---

# Common Mistakes

- Missing `[SessionAuthorize]` on authenticated endpoints
- SteamId type mismatch (`ulong` vs `long`)
- Modifying Entity fields directly instead of through domain methods
- Hardcoding Redis keys instead of using `CacheKey`
- Controller calling Repository directly
- Service referencing concrete class instead of Interface
- EF mapping via DataAnnotation instead of `IEntityTypeConfiguration<T>`
- Missing DI registration after adding a new service
