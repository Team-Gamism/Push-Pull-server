# Test Filter Patterns — Push & Pull Server

## How filtering works

```bash
dotnet test --filter "FullyQualifiedName~{value}"
```

The `~` operator matches any fully qualified test name that **contains** the value as a substring.

Fully qualified name format:
```
{Namespace}.{OuterClass}+{InnerClass}.{MethodName}
```

Example:
```
PushAndPull.Test.Service.Auth.LoginServiceTests+WhenANewUserLogsInForTheFirstTime.It_CreatesANewUser
```

---

## Current test classes in this project

| `/test` argument | What runs |
|---|---|
| _(no argument)_ | All tests |
| `LoginServiceTests` | All LoginService tests |
| `LogoutServiceTests` | All LogoutService tests |
| `CreateRoomServiceTests` | All CreateRoomService tests |
| `GetAllRoomServiceTests` | All GetAllRoomService tests |
| `GetRoomServiceTests` | All GetRoomService tests |
| `JoinRoomServiceTests` | All JoinRoomService tests |
| `WhenANewUserLogsIn` | New user login scenario |
| `WhenAnExistingUserLogsIn` | Existing user login scenario |
| `It_CreatesANewUser` | Specific test method |

---

## Pattern examples

### Filter by outer class — runs all tests for a service
```
/test LoginServiceTests
```
→ Matches all of `PushAndPull.Test.Service.Auth.LoginServiceTests+*.*`

### Filter by inner class — runs all tests in a scenario
```
/test WhenANewUserLogsInForTheFirstTime
```
→ Matches all methods inside that inner class

### Filter by method name — runs a single test
```
/test It_CreatesANewUser
```
→ Matches the single method by name substring

### Filter by service name
```
/test JoinRoom
```
→ Matches all of `PushAndPull.Test.Service.Room.JoinRoomServiceTests+*.*`

### Filter by namespace — runs an entire domain
```
/test PushAndPull.Test.Service.Auth
```
→ Matches everything under the Auth service namespace
