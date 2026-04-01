# Commit Type Guide — Push & Pull Server

## feat — New capability added to the codebase

Use when creating new files is the primary change.

**Examples from this project:**

| Change | Commit message |
|---|---|
| Add LoginService.cs, LogoutService.cs | `feat: 로그인·로그아웃 서비스 추가` |
| Add RoomController.cs | `feat: Room 컨트롤러 추가` |
| Add JoinRoomService.cs | `feat: 방 참여 서비스 추가` |
| Add new Entity class | `feat: {EntityName} 엔터티 추가` |
| Add new test class | `feat: {ServiceName} 테스트 추가` |
| Add new migration file | `feat: {MigrationName} 마이그레이션 추가` |

---

## fix — Broken behavior or missing registration/config corrected

Use when existing code is wrong, or a required wiring (DI, config key) is absent.
Adding only a DI registration line without adding the service file itself is also `fix`.

**Examples from this project:**

| Change | Commit message |
|---|---|
| Add missing `services.AddScoped<IRoomRepository, RoomRepository>()` | `fix: IRoomRepository DI 누락 수정` |
| Fix wrong CacheKey prefix | `fix: 세션 캐시 키 prefix 수정` |
| Fix SteamId type mismatch (ulong vs long) | `fix: SteamId 타입 불일치 수정` |
| Fix missing `[SessionAuthorize]` on endpoint | `fix: 방 참여 인증 누락 수정` |

---

## update — Existing code modified without adding a new capability

Use when modifying files that already exist — renaming, restructuring, adjusting behavior.

**Examples from this project:**

| Change | Commit message |
|---|---|
| Change response type to `CommonApiResponse<T>` | `update: 방 생성 응답 타입을 CommonApiResponse로 변경` |
| Modify entity domain method | `update: Room 참여 로직 수정` |
| Add a test method to an existing test class | `update: LoginService 테스트 추가` |
| Refactoring without behavior change | `update: RoomService 리팩토링` |
| Update appsettings connection string | `update: DB 연결 문자열 수정` |

---

## Boundary rules

| Situation | Type |
|---|---|
| New `.cs` service/repository/controller file added | `feat` |
| New method added to an existing `.cs` file | `update` |
| DI registration line added alone, no new service file | `fix` |
| New service file + its DI registration added together | `feat` (same logical unit) |
| New migration file added | `feat` |
| Existing migration file corrected | `fix` |
| New test class added | `feat` |
| Test method added to an existing test class | `update` |
| Refactoring without behavior change | `update` |

---

## When to split into multiple commits

```
# New service + its DI registration → one logical unit, commit together
git add Domain/Room/Service/JoinRoomService.cs Domain/Room/Config/RoomServiceConfig.cs
git commit -m "feat: 방 참여 서비스 추가"

# Separate DI fix → independent fix
git add Domain/Auth/Config/AuthServiceConfig.cs
git commit -m "fix: ISessionService DI 누락 수정"
```
