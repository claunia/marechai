---
name: dotnet-migration-generator
description: Generate, validate, and document EF Core database migrations with proper naming and structure
disable-model-invocation: false
user-invocable: true
---

# EF Core Migration Generator

Generate and manage Entity Framework Core migrations for the Marechai.Database project with proper naming conventions and validation.

## Usage

Invoke with a description of the schema change:

```
/dotnet-migration-generator Add support for storing user preferences with new UserPreference table
```

Or validate an existing migration:

```
/dotnet-migration-generator Validate the latest migration in Marechai.Database
```

## What This Skill Does

1. **Generate Migrations**: Creates new EF Core migrations with semantic names based on the change description
2. **Validate Structure**: Ensures migrations follow project conventions (naming, methods, Up/Down implementations)
3. **Document Changes**: Generates migration summary showing entities, properties, and relationships affected
4. **Verify Compilation**: Checks that migrations compile and don't conflict with existing ones

## How to Use

### Generate a New Migration

Tell the skill what database schema change you want:

> I need to add a nullable `Biography` field to the `People` table and a new `PersonSource` enum type.

The skill will:
1. Update relevant DbContext models in `Marechai.Database/Data/`
2. Run `dotnet ef migrations add <MigrationName>` with a semantic name
3. Review the generated migration file for correctness
4. Compile and validate with `dotnet build Marechai.Database.csproj`
5. Output a summary of what changed

### Validate Migrations

Ask to review migrations before commit:

> Validate the last 3 migrations to ensure they're safe

The skill checks:
- Migration naming follows `<YYYYMMDD>_<SemanticName>` or similar convention
- `Up()` and `Down()` methods are balanced
- No breaking changes without data migration strategies
- SQL syntax is correct for MySQL/MariaDB

### Document Migration Impact

> Document the migration for the new ComputerFamily feature

The skill generates a summary showing:
- Entities modified/created
- Properties added/changed/removed
- Relationship changes
- Default values and constraints

## Project Context

**Database Project**: `Marechai.Database`
**DbContext**: `Marechai.Database/Data/MarechaiDbContext.cs`
**Models Location**: `Marechai.Database/Data/Models/`
**Migrations Location**: `Marechai.Database/Migrations/`
**Database**: MySQL/MariaDB via Pomelo.EntityFrameworkCore.MySql

**Key Files**:
- `.editorconfig` - C# formatting rules (migrations must comply)
- `Directory.Packages.props` - Central version management (EF Core 9.0.11, Pomelo 9.0.0)

## Migration Naming Conventions

Migrations in this project follow a semantic naming pattern:

- `<YYYYMMDD>_<PascalCaseDescription>` (e.g., `20250612_AddUserPreferencesTable`)
- Names should reflect the primary change (Add, Remove, Refactor, Rename)
- Avoid overly long names; focus on the main entity/change

## Best Practices

1. **Small, Focused Migrations**: Each migration should address one logical change
2. **Data Migrations**: If adding NOT NULL columns to existing tables, include data seeding in the migration
3. **Reversibility**: Both `Up()` and `Down()` must be implemented correctly
4. **Testing**: Run migrations locally before committing
5. **Documentation**: Complex migrations should include comments in the Up/Down methods

## Pre-Migration Checklist

Before generating a migration:

- [ ] Verify the DbContext model changes are complete
- [ ] Check for any data type compatibility with MySQL/MariaDB
- [ ] Consider impact on existing data (constraints, nullability, foreign keys)
- [ ] Ensure the migration doesn't break existing relationships

## Commands This Skill Uses

```bash
# View pending migrations
dotnet ef migrations list --project Marechai.Database

# Add a new migration
dotnet ef migrations add <MigrationName> --project Marechai.Database

# Update database with migrations
dotnet ef database update --project Marechai.Database

# Remove last migration (if not applied)
dotnet ef migrations remove --project Marechai.Database

# Build and validate
dotnet build Marechai.Database.csproj
```

## Examples

**Adding a new table:**
```
/dotnet-migration-generator Add a new SoftwareLicense table with Name, Abbreviation, and URL properties
```

**Modifying an existing entity:**
```
/dotnet-migration-generator Add ReleaseDate property to the Software entity as nullable DateTime
```

**Refactoring relationships:**
```
/dotnet-migration-generator Rename PersonCompany.Company navigation to Companies and update foreign key references
```

**Data migration:**
```
/dotnet-migration-generator Populate the new Status enum field for existing records with default value 'Active'
```
