---
description: Add an EF Core migration and optionally update the database
allowed-tools: Bash, Read
---

Add an EF Core migration using the name provided as the argument.

Migration name must follow the convention: `{Verb}{Target}{Change}` (e.g. `AddRoomPasswordColumn`, `RemoveUserNicknameColumn`).

Usage: `/migrate <MigrationName>`

## Steps

1. Run the migration command using the argument as the migration name:
   ```bash
   dotnet ef migrations add $ARGUMENTS --project PushAndPull
   ```
2. After the migration is created:
   - Read the generated migration file under `PushAndPull/Migrations/`
   - Briefly summarize what the migration does (tables created/altered, columns added/removed, indexes, etc.)
3. Ask the user whether to apply the migration to the database:
   - If yes, run:
     ```bash
     dotnet ef database update --project PushAndPull
     ```
   - Confirm the update result when done
