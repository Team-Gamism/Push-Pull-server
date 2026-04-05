---
description: C# code style rules. Applied for all .cs files.
globs: ["**/*.cs"]
alwaysApply: false
---

## C# Code Style

### General

- Use `var` only when the type is obvious from the right-hand side.
- Prefer expression-body methods for single-line implementations.
- No XML doc comments unless explicitly requested.
- No `#region` blocks.

### Naming

- Private fields: `_camelCase` (underscore prefix)
- Properties, methods, classes: `PascalCase`
- Local variables and parameters: `camelCase`
- Interfaces: `IPascalCase`
- Database column names: `snake_case`

### Classes & Constructors

- Constructor-inject all dependencies; assign to `private readonly` fields.
- Keep constructors minimal — no logic, only assignments.

### DTOs

- Always use `record` types.
- Request DTOs in `Dto/Request/`, Response DTOs in `Dto/Response/`.

```csharp
public record CreateRoomRequest(
    long LobbyId,
    string RoomName,
    bool IsPrivate,
    string? Password
);
```

### Entities

- Default constructor: `protected` or `private`.
- Public constructor accepts required fields only.
- State changes only through domain methods (`room.Join()`, `user.UpdateNickname()`).
- No direct field mutation from outside the entity.

### Service Command/Result Pattern

- Each service interface defines its own `Command` (input) and `Result` (output) records.
- Co-located in the same Interface file.

### Async

- All I/O methods are `async Task` or `async Task<T>`.
- Never use `.Result` or `.Wait()`.
- Always `await` — no fire-and-forget unless intentional.
- Pass `CancellationToken` through to repository/async calls where appropriate.

### Exception Handling

- Throw domain-specific exceptions (e.g., `NotFoundException`, `UnauthorizedException` from `Gamism.SDK`).
- Do not catch and re-throw unless adding context.
- No empty catch blocks.
