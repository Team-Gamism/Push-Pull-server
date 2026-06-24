---
name: db-migrate
description: Generate and review an EF Core migration for the PushAndPull project. Builds first, runs `dotnet ef migrations add` with the correct project paths, reviews the generated Up/Down for unintended schema churn, and covers remove/redo. Does NOT apply migrations to a database.
---

Use this skill whenever an entity or `IEntityTypeConfiguration` change requires a schema migration. Scope is **generate + review only** — applying to a database (`dotnet ef database update`) is handled by CI / manual deployment and is out of scope here.

All commands run from the repository root. The API project doubles as the migrations and startup project:

- Project / startup project: `src/PushAndPull/PushAndPull.csproj`
- Migrations live in that project's `Migrations/` folder (the default output dir — do not override it).
- `migrations add` does **not** connect to a database, so no `DB_CONNECTION_STRING` is needed to generate.

## Step 1 — Build first

A migration is generated from the compiled model, so the solution must build.

```bash
dotnet build src/PushAndPull.sln --nologo
```

Fix any build errors before continuing.

## Step 2 — Add the migration

Choose a descriptive PascalCase name that reflects the schema change (e.g. `AddGuestLastHeartbeatAt`, `CreateRoomTable`).

```bash
dotnet ef migrations add <PascalCaseName> \
  --project src/PushAndPull/PushAndPull.csproj \
  --startup-project src/PushAndPull/PushAndPull.csproj
```

This creates `Migrations/<timestamp>_<PascalCaseName>.cs`, its `.Designer.cs`, and updates `AppDbContextModelSnapshot.cs`.

## Step 3 — Review the generated migration (required)

Open `Migrations/<timestamp>_<PascalCaseName>.cs` and confirm `Up`/`Down` contain **only** the schema change you intended.

- Each operation must trace to your entity/config change. Watch for snake_case column/table names and `timestamptz` for `DateTimeOffset` (project conventions).
- If unrelated operations appear (columns, tables, indexes, or type changes you did not make), the model snapshot had drifted. Stop and investigate before committing — do not ship unexplained churn.

## Step 4 — Verify

Per the project verification rules, after adding the migration:

```bash
dotnet build src/PushAndPull.sln --nologo
dotnet test src/PushAndPull.sln --nologo
```

## If the migration is wrong — remove and redo

Only safe when the migration has **not** been committed/shared or applied to any database (committed migration history must not be rewritten).

```bash
dotnet ef migrations remove \
  --project src/PushAndPull/PushAndPull.csproj \
  --startup-project src/PushAndPull/PushAndPull.csproj
```

Then adjust the entity/config and repeat from Step 1. For a schema change on top of an **already-committed** migration, add a new migration instead of editing the old one.
