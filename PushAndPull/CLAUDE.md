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
PushAndPull/
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
dotnet build PushAndPull/PushAndPull.csproj
```

## Run Locally

```bash
dotnet run --project PushAndPull/PushAndPull.csproj
```

## EF Core Migration

```bash
dotnet ef migrations add <MigrationName> --project PushAndPull
```

## Update Database

```bash
dotnet ef database update --project PushAndPull
```

## Docker Build

```bash
docker build -f PushAndPull/Dockerfile -t pushandpull-server .
```

## Docker Run

```bash
docker run -p 8080:8080 pushandpull-server
```

---

# Key Paths

Controllers

```
PushAndPull/Domain/{DomainName}/Controller/
```

Request / Response DTO

```
PushAndPull/Domain/{DomainName}/Dto/Request/
PushAndPull/Domain/{DomainName}/Dto/Response/
```

Services

```
PushAndPull/Domain/{DomainName}/Service/
```

Service Interfaces (+ Command / Result records)

```
PushAndPull/Domain/{DomainName}/Service/Interface/
```

Domain Entities

```
PushAndPull/Domain/{DomainName}/Entity/
```

Domain Exceptions

```
PushAndPull/Domain/{DomainName}/Exception/
```

Repositories

```
PushAndPull/Domain/{DomainName}/Repository/
PushAndPull/Domain/{DomainName}/Repository/Interface/
```

Service DI Registration

```
PushAndPull/Domain/{DomainName}/Config/
```

DbContext

```
PushAndPull/Global/Infrastructure/AppDbContext.cs
```

Redis Cache

```
PushAndPull/Global/Cache/
```

Steam Authentication

```
PushAndPull/Global/Auth/
```

Security (SessionAuthorize, ClaimsExtensions)

```
PushAndPull/Global/Security/
```

Shared Utilities (PasswordHasher, RoomCodeGenerator 등)

```
PushAndPull/Global/Service/
```

Global DI Registration

```
PushAndPull/Global/Config/GlobalServiceConfig.cs
```

---

# Architecture

이 프로젝트는 **도메인 중심 레이어드 아키텍처**를 따른다.

각 도메인(`Auth`, `Room`)은 독립적인 폴더 아래에 Controller, Service, Repository, Entity, Dto, Exception, Config를 포함한다.

도메인 횡단 관심사(캐시, 인증, DB, 보안, 유틸)는 `Global/`에 위치한다.

의존성 방향:

```
Controller → Service Interface
Service → Repository Interface
Service → Global Service Interface (IAuthTicketValidator, ISessionService 등)
Repository → AppDbContext
```

금지 의존성:

```
Controller → Repository ❌
Controller → 구현 클래스 직접 참조 ❌
Service → Repository 구현 클래스 직접 참조 ❌
Entity → Service / Repository ❌
```

---

# Layer Responsibilities

## Domain/{DomainName}

각 도메인 폴더가 해당 기능의 전체 레이어를 포함한다.

### Controller

* HTTP 엔드포인트 정의
* Request DTO → Command 변환
* Service 호출 후 Result → Response DTO 변환
* `[SessionAuthorize]`로 인증 처리

Controller는 **Service Interface**에만 의존해야 한다.

### Service / Interface

비즈니스 로직 구현.

* Service Interface 파일에 `Command` (입력) 와 `Result` (출력) record를 함께 정의
* Service는 `ExecuteAsync(...)` 메서드를 통해 유스케이스를 노출
* Repository Interface와 Global Service Interface에만 의존

예시:

```csharp
// Interface 파일
public interface ILoginService
{
    Task<LoginResult> ExecuteAsync(LoginCommand request);
}

public record LoginCommand(string Ticket, string Nickname);
public record LoginResult(string SessionId);
```

### Repository / Interface

데이터 접근 추상화.

* Interface는 `Repository/Interface/`에 위치
* 구현체는 EF Core 또는 Dapper 사용
* 읽기 쿼리는 반드시 `AsNoTracking()` 사용

### Entity

순수 도메인 모델.

* 외부 의존성 없음
* 비즈니스 로직은 Entity 메서드로 캡슐화
* 상태 변경은 반드시 메서드를 통해

예시:

```
room.Join()
room.Close()
user.UpdateNickname()
```

### Dto

* `Request/` — HTTP 요청 DTO
* `Response/` — HTTP 응답 DTO
* 모두 `record` 타입 사용

### Exception

도메인별 예외 클래스.

### Config

해당 도메인의 서비스 DI 등록 Extension 메서드.

---

## Global/

도메인 횡단 관심사.

| 폴더 | 역할 |
|------|------|
| `Auth/` | Steam 티켓 검증 (`IAuthTicketValidator`, `SteamAuthTicketValidator`) |
| `Cache/` | Redis 캐시 (`ICacheStore`, `CacheStore`, `CacheKey`) |
| `Config/` | 전역 DI 등록, DB/Redis 설정 |
| `Infrastructure/` | `AppDbContext` (EF Core) |
| `Security/` | `[SessionAuthorize]`, `ClaimsPrincipalExtensions` |
| `Service/` | 공용 유틸 (`IPasswordHasher`, `IRoomCodeGenerator` 등) |

---

# Coding Conventions

## General C#

DTO 타입

```
record
```

Domain Entity / Service

```
class
```

의존성 주입:

```
생성자 주입만 사용
```

Nullable reference types 활성화.

Implicit usings 활성화.

---

## Naming

Service Interface

```
ILoginService
ICreateRoomService
```

Repository Interface

```
IUserRepository
IRoomRepository
```

Command (Service 입력)

```
LoginCommand
CreateRoomCommand
```

Result (Service 출력)

```
LoginResult
CreateRoomResult
```

Request DTO

```
LoginRequest
CreateRoomRequest
```

Response DTO

```
LoginResponse
CreateRoomResponse
```

Service 구현체

```
LoginService
CreateRoomService
```

---

## Entity Design

규칙:

* 기본 생성자는 `protected` 또는 `private`
* Public 생성자는 필수 필드만 받음
* 상태 변경은 메서드를 통해서만

Private setter를 사용하여 외부 수정 방지.

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

예시:

```
.HasConversion<string>()
```

읽기 쿼리는 반드시:

```
AsNoTracking()
```

EF 매핑은 반드시 `AppDbContext.OnModelCreating`에서만 설정.

DataAnnotations로 EF 매핑하는 것은 금지.

---

# Cache Rules

Redis는 **세션 저장소**로 사용.

모든 키는 반드시:

```
CacheKey
```

를 통해 생성.

금지:

```
하드코딩 Redis 키
```

올바른 예시:

```
CacheKey.Session.ById(sessionId)
```

---

# Authentication

Steam 인증:

```
ISteamUserAuth/AuthenticateUserTicket/v1
```

세션 인증은 HTTP 헤더를 통해:

```
Session-Id: 3c4e2b1d...
```

Bearer 토큰은 **사용하지 않음**.

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

세션 인증이 필요한 엔드포인트에 `[SessionAuthorize]` 누락.

SteamId 타입 혼동:

```
User.SteamId → ulong
Claims → long
```

Domain 메서드를 통하지 않고 Entity 필드를 직접 수정.

`CacheKey` 대신 Redis 키를 하드코딩.

Controller가 Repository를 직접 호출.

Service가 구현 클래스를 직접 참조 (Interface를 통해야 함).

`OnModelCreating` 대신 DataAnnotation으로 EF 매핑.

새 서비스 추가 후 `Program.cs` 또는 `{Domain}ServiceConfig`에 DI 등록 누락.

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

* Description은 **한국어**로 작성
* 단문, 명령형
* 끝에 구두점 금지 (`.`, `!` 등)

Examples:

```
feat: 방 생성 API 추가
fix: 세션 DI 누락 수정
modify: Room 엔터티 수정
```

---

# Claude Guidelines

코드 생성 또는 수정 시:

1. 도메인 중심 레이어드 아키텍처를 엄수한다.
2. Controller는 **Service Interface**에만 의존해야 한다.
3. Service는 **Repository Interface** 및 **Global Interface**에만 의존해야 한다.
4. Domain Entity는 비즈니스 로직을 캡슐화해야 한다.
5. DTO는 `record` 타입을 사용한다.
6. EF 매핑은 `OnModelCreating`에서만 설정한다.
7. Domain 로직을 우회하지 않는다.

새 기능 추가 시:

1. `Domain/{DomainName}/Service/Interface/`에 Service Interface와 Command/Result record 정의.
2. `Domain/{DomainName}/Service/`에 Service 구현체 작성.
3. 필요한 경우 `Domain/{DomainName}/Repository/Interface/`에 Repository Interface 추가.
4. `Domain/{DomainName}/Repository/`에 Repository 구현체 작성.
5. `Domain/{DomainName}/Controller/`에 Controller 엔드포인트 추가.
6. `Domain/{DomainName}/Config/`의 DI 등록 메서드에 새 서비스 등록.
