---
description: EF Core migration guide (.NET/C# - PushAndPull)
---

# EF Core Migration Guide

## Entity Change Checklist

- [ ] Analyzed impact on existing data
- [ ] Determined need for migration script
- [ ] Planned 2-phase deployment strategy for column deletion
- [ ] Prepared rollback strategy

## Change Order

1. **Modify Entity**: `Domain/{DomainName}/Entity/`
2. **Modify AppDbContext**: `OnModelCreating` in `Global/Infrastructure/AppDbContext.cs`
3. **Modify DTO**: Update `Request/`, `Response/` records
4. **Modify Repository**: Adjust queries if needed
5. **Modify Service**: Update business logic
6. **Generate and apply migration**

## EF Core Mapping Rules

All EF mappings must be configured in a dedicated `IEntityTypeConfiguration<T>` class per entity. DataAnnotations are forbidden.

**File location:** `Domain/{DomainName}/Entity/Config/{Entity}Config.cs`

```csharp
public class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("room", "room");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Status)
               .HasColumnName("status")
               .HasConversion<string>();       // Enum → string
        builder.Property(e => e.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamptz"); // timestamp with time zone
    }
}
```

`AppDbContext.OnModelCreating` calls only one line — all configs are auto-discovered:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

### Column Naming Rules

| C# Property | DB Column |
|-------------|-----------|
| `Id` | `id` |
| `RoomCode` | `room_code` |
| `CreatedAt` | `created_at` |
| `SteamId` | `steam_id` |

- DB column names: `snake_case`
- Timestamp type: `timestamptz` (stores UTC, converts to local timezone on read)
- Enum storage: `.HasConversion<string>()`

## Migration Commands

```bash
# Generate migration file
dotnet ef migrations add <MigrationName> --project PushAndPull

# Apply to database
dotnet ef database update --project PushAndPull

# List all migrations
dotnet ef migrations list --project PushAndPull

# Rollback to a specific migration
dotnet ef database update <PreviousMigrationName> --project PushAndPull

# Remove last migration (only if not yet applied)
dotnet ef migrations remove --project PushAndPull
```

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
    public string? Description { get; private set; }  // add as nullable

    public void UpdateDescription(string description)
    {
        Description = description;
    }
}

// 2. Add mapping in OnModelCreating
entity.Property(e => e.Description)
      .HasColumnName("description")
      .IsRequired(false);

// 3. Generate migration
// dotnet ef migrations add AddRoomDescriptionColumn --project PushAndPull
```

### Deleting a Column (2-Phase Deployment)

```
Phase 1 — Deprecate:
  - Keep the Entity property
  - Stop using the column in new code
  - Run data migration if needed

Phase 2 — Delete:
  - Remove the Entity property
  - Remove mapping from OnModelCreating
  - dotnet ef migrations add Remove{Column}Column
```

### Adding / Changing an Enum

```csharp
// 1. Define or modify the Enum
public enum RoomStatus { Open, Closed, Full }

// 2. Keep string conversion in OnModelCreating
entity.Property(e => e.Status).HasConversion<string>();

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

## Important Notes

- **Never map EF via DataAnnotations** — use `OnModelCreating` only
- Always use `AsNoTracking()` for read-only queries
- Do not manually edit migration files (share with team if unavoidable)
- Always use 2-phase deployment before deleting a column
