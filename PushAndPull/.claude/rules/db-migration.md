---
description: EF Core migration workflow, naming, and entity modification patterns. Applied when modifying Entity or Entity/Config files.
globs: ["**/Entity/**/*.cs", "**/Entity/Config/**/*.cs"]
alwaysApply: false
---

## Entity Change Checklist

- [ ] Analyzed impact on existing data
- [ ] Determined need for migration script
- [ ] Planned 2-phase deployment strategy for column deletion
- [ ] Prepared rollback strategy

## Change Order

1. **Modify Entity**: `Domain/{DomainName}/Entity/`
2. **Modify EF Config**: `Domain/{DomainName}/Entity/Config/{Entity}Config.cs`
3. **Modify DTO**: Update `Request/`, `Response/` records
4. **Modify Repository**: Adjust queries if needed
5. **Modify Service**: Update business logic
6. **Generate and apply migration**: `/db-migrate add <MigrationName>`

## Migration Name Convention

```
{Verb}{Target}{Change}

Examples:
AddRoomPasswordColumn
RemoveUserNicknameColumn
RenameRoomStatusToState
AddIndexOnRoomCode
CreateUserTable
```

## Entity Modification Patterns

### Adding a Column

```csharp
// 1. Add property to Entity
public class Room
{
    public string? Description { get; private set; }

    public void UpdateDescription(string description)
    {
        Description = description;
    }
}

// 2. Add mapping in Config
builder.Property(e => e.Description)
       .HasColumnName("description")
       .IsRequired(false);

// 3. Generate migration
// /db-migrate add AddRoomDescriptionColumn
```

### Deleting a Column (2-Phase Deployment)

```
Phase 1 — Deprecate:
  - Keep the Entity property
  - Stop using the column in new code
  - Run data migration if needed

Phase 2 — Delete:
  - Remove the Entity property
  - Remove mapping from Config
  - /db-migrate add Remove{Column}Column
```

### Adding / Changing an Enum

```csharp
// 1. Define or modify the Enum
public enum RoomStatus { Open, Closed, Full }

// 2. Keep string conversion in Config
builder.Property(e => e.Status).HasConversion<string>();

// 3. Verify that existing string values in DB match the Enum member names
```

## Rollback Strategy

```bash
# Rollback DB to a previous migration
dotnet ef database update <PreviousMigrationName> --project PushAndPull

# Remove the migration file after rollback (local only)
dotnet ef migrations remove --project PushAndPull
```

For production rollbacks:
- Always verify the `Down()` method in the migration file is correctly implemented
- If there is a risk of data loss (`DropColumn`, `DropTable`), take a backup first
