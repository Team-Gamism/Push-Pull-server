# Migration Naming Guide — Push & Pull Server

## Format

**PascalCase**, no spaces, clearly describes the schema operation being performed.

---

## Naming patterns by operation

### New table
```
Create{TableName}Table
```
e.g. `CreateRoomTable`, `CreateUserTable`, `CreateRoomMemberTable`

### Initial schema (multiple tables at once)
```
CreateTables
InitialSchema
```

### Add column to existing table
```
Add{ColumnName}To{TableName}
```
e.g. `AddPasswordToRoom`, `AddNicknameToUser`, `AddStatusToRoom`

### Remove column
```
Remove{ColumnName}From{TableName}
```
e.g. `RemoveDeprecatedFieldFromRoom`

### Fix column type, constraint, or misconfiguration
```
Fix{ColumnName}{Issue}
Change{ColumnName}In{TableName}
```
e.g.
- `FixRoomCodeUniqueConstraint`
- `ChangeStatusColumnTypeInRoom`

### Add index
```
Add{Description}IndexTo{TableName}
```
e.g. `AddRoomCodeUniqueIndexToRoom`, `AddSteamIdIndexToUser`

### Add foreign key
```
Add{Relation}ForeignKeyTo{TableName}
```
e.g. `AddRoomForeignKeyToRoomMember`

---

## Anti-patterns (avoid)

| Bad | Good |
|---|---|
| `Migration1` | `CreateRoomTable` |
| `FixRoom` | `FixRoomCodeUniqueConstraint` |
| `UpdateSchema` | `AddPasswordToRoom` |
| `Temp` | (describe what actually changed) |
| `Fix` | `FixCreatedAtColumnTypeInRoom` |
