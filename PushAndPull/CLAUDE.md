# CLAUDE.md — Push & Pull Server

**Always respond in Korean.**

This document provides guidance for Claude Code when working with this repository.

---

# Project Overview

Push & Pull Server is a **Steam-based multiplayer matchmaking / lobby server**.

Players authenticate using **Steam authentication tickets**, create or join rooms, and exchange **Steam Lobby IDs**.

Actual gameplay networking happens through **Steam P2P**, while this server only manages:

* Session authentication
* Room creation / join
* Active room discovery

## Solution Structure

```
PushAndPull.sln
```

Main project:

```
Server/
```

Target framework:

```
net9.0
```

Docker runtime:

```
Linux container
Port: 8080
```

---

# Commands

## Build

```bash
dotnet build Server/Server.csproj
```

## Run Locally

```bash
dotnet run --project Server/Server.csproj
```

## EF Core Migration

```bash
dotnet ef migrations add <MigrationName> --project Server
```

## Update Database

```bash
dotnet ef database update --project Server
```

## Docker Build

```bash
docker build -f Server/Dockerfile -t pushandpull-server .
```

## Docker Run

```bash
docker run -p 8080:8080 pushandpull-server
```

---

# Key Paths

Controllers

```
Server/Api/Controller/
```

Request / Response DTO

```
Server/Api/Dto/
```

UseCases

```
Server/Application/UseCase/
```

Input Ports

```
Server/Application/Port/Input/
```

Output Ports

```
Server/Application/Port/Output/
```

Domain Entities

```
Server/Domain/Entity/
```

Repositories

```
Server/Infrastructure/Persistence/Repository/
```

DbContext

```
Server/Infrastructure/Persistence/DbContext/
```

Redis Cache

```
Server/Infrastructure/Cache/
```

Steam Authentication

```
Server/Infrastructure/Auth/
```

---

# Architecture

This project strictly follows **Hexagonal Architecture (Ports & Adapters)**.

Dependency direction:

```
Api → Application → Domain
Infrastructure → Application (via Port interfaces)
```

Forbidden dependencies:

```
Application → Infrastructure ❌
Domain → Infrastructure ❌
Domain → Application ❌
```

---

# Layer Responsibilities

## Domain

Pure domain model layer.

Rules:

* No external dependencies
* Business logic belongs in Entities
* State changes must be done through methods

Example:

```
room.Join(player)
room.Close()
```

Entities:

```
User
Room
PlayerSession
```

Exceptions are defined in:

```
Domain/Exception/
```

---

## Application

Business use cases.

Contains:

```
UseCases
Input Ports
Output Ports
Application Services
```

Rules:

* UseCases expose `ExecuteAsync(...)`
* UseCases depend only on **Port interfaces**
* Infrastructure implementations must not be referenced

DTO rules:

```
Command → input
Result → output
```

Example:

```
CreateRoomCommand
CreateRoomResult
```

---

## Infrastructure

External system integrations.

Contains:

```
EF Core repositories
Redis cache
Steam Web API integration
Password hashing
Room code generation
```

Rules:

* Implements **Output Ports**
* Handles all external IO
* No business logic

Database mapping must be defined only in:

```
AppDbContext.OnModelCreating
```

DataAnnotations for EF mapping are forbidden.

---

## API

Presentation layer.

Responsibilities:

* HTTP endpoints
* Request → Command mapping
* Result → Response mapping
* Session authentication

Rules:

Controllers must depend only on **UseCase interfaces**.

Forbidden:

```
Controller → Repository ❌
Controller → Infrastructure Service ❌
```

Authentication:

```
[SessionAuthorize]
```

User identity extraction:

```
User.GetSessionId()
User.GetSteamId()
```

---

# Coding Conventions

## General C#

DTO types

```
record
```

Domain / Services

```
class
```

Dependency injection:

```
Constructor injection only
```

Nullable reference types enabled.

Implicit usings enabled.

---

## Naming

Interfaces

```
ILoginUseCase
IRoomRepository
```

Commands

```
CreateRoomCommand
JoinRoomCommand
```

Results

```
CreateRoomResult
```

Request DTO

```
CreateRoomRequest
```

Response DTO

```
CreateRoomResponse
```

UseCases

```
CreateRoomUseCase
JoinRoomUseCase
```

---

## Entity Design

Rules:

* Default constructor must be `protected` or `private`
* Public constructor receives required fields only
* State mutation via methods only

Example:

```
room.Join()
room.Close()
user.UpdateNickname()
```

Private setters must be used to prevent external modification.

---

# Database Rules

Database:

```
PostgreSQL
```

ORM:

```
EF Core 9
```

Naming:

```
snake_case
```

Timestamp:

```
timestamptz
```

Enum storage:

```
string conversion
```

Example:

```
.HasConversion<string>()
```

Read queries must use:

```
AsNoTracking()
```

---

# Cache Rules

Redis is used for **session storage**.

All keys must be generated through:

```
CacheKey
```

Forbidden:

```
Hardcoded Redis keys
```

Correct example:

```
CacheKey.Session.ById(sessionId)
```

---

# Authentication

Steam authentication uses:

```
ISteamUserAuth/AuthenticateUserTicket/v1
```

Session authentication is performed using:

```
Session-Id HTTP header
```

Example:

```
Session-Id: 3c4e2b1d...
```

Bearer tokens are **not used**.

---

# API Endpoints

| Method | Path                         | Auth    | Description        |
| ------ | ---------------------------- | ------- | ------------------ |
| POST   | /api/v1/auth/login           | No      | Steam ticket login |
| POST   | /api/v1/auth/logout          | Session | Logout             |
| POST   | /api/v1/room                 | Session | Create room        |
| GET    | /api/v1/room/all             | No      | List active rooms  |
| GET    | /api/v1/room/{roomCode}      | No      | Get room info      |
| POST   | /api/v1/room/{roomCode}/join | Session | Join room          |

---

# Common Mistakes

Missing session authentication on protected endpoints.

SteamId type confusion:

```
User.SteamId → ulong
Claims → long
```

Updating entity fields directly instead of using domain methods.

Hardcoding Redis keys instead of using `CacheKey`.

Controllers calling repositories directly.

Application layer referencing Infrastructure classes.

Adding EF Core mapping attributes instead of Fluent API configuration.

Forgetting to register new services in `Program.cs`.

---

# Commit Message Style

Format:

```
{type}: {한국어 설명}
```

Types:

```
feat    → 새 기능 추가
fix     → 버그 수정, 누락된 설정/DI 추가
modify  → 기존 코드 수정
merge   → 브랜치 머지
release → 릴리즈 (release/x.x.x 형식)
```

Rules:

* Description is written in **Korean**
* Short and imperative (단문)
* No trailing punctuation (`.`, `!` 등 금지)

Examples:

```
feat: 방 생성 API 추가
fix: 세션 DI 누락 수정
modify: Room 엔터티 수정
```

---

# Claude Guidelines

When generating or modifying code:

1. Follow **Hexagonal Architecture** strictly.
2. Controllers must depend only on **UseCase interfaces**.
3. UseCases must not access Infrastructure directly.
4. Domain entities must encapsulate business logic.
5. DTOs must use `record` types.
6. EF mapping must be configured only in `OnModelCreating`.
7. Do not bypass Domain logic.

When adding new features:

1. Define **Input Port (UseCase interface)**.
2. Implement **UseCase in Application layer**.
3. Define required **Output Ports**.
4. Implement adapters in **Infrastructure layer**.
5. Expose via **API Controller**.
