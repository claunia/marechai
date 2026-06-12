---
name: ef-dbcontext-analyzer
description: Analyze and document Entity Framework Core DbContext structure, models, and relationships
disable-model-invocation: false
user-invocable: true
---

# EF Core DbContext Analyzer

Analyze and document the Entity Framework Core structure, entities, relationships, and configurations in the Marechai project.

## Usage

Invoke to understand or document your data model:

```
/ef-dbcontext-analyzer Analyze the relationship between Computer and Manufacturer entities
```

Or generate comprehensive documentation:

```
/ef-dbcontext-analyzer Document the complete data model for the Software and License domain
```

## What This Skill Does

1. **Analyze Entities**: Maps out all DbSet properties, their models, and relationships
2. **Document Relationships**: Shows one-to-many, many-to-many, and one-to-one relationships
3. **Identify Constraints**: Documents primary keys, foreign keys, unique constraints, and indexes
4. **Generate ER Diagrams**: Creates a text-based entity relationship overview
5. **Validate Configuration**: Checks fluent API configurations and shadow properties
6. **Find Cross-Cutting Concerns**: Identifies shared patterns (soft delete, timestamps, audit fields)

## How to Use

### Understand an Entity

Ask about a specific entity to understand its structure and relationships:

> Analyze the Computer entity and show all its relationships to other entities

The skill will show:
- Properties (type, nullable, constraints)
- Navigation properties and relationships
- Foreign key configurations
- Any index or unique constraints
- How it's used by other entities

### Explore a Domain Area

Understand a logical domain within the data model:

> Map out the entire Software domain including Software, Genre, Publisher, and License relationships

The skill generates:
- Entity definitions and properties
- How they relate to each other
- Data flow between entities
- Any shared configurations or patterns

### Generate Documentation

Create reference documentation for new developers:

> Generate ER diagram documentation for the Media and Storage entities

Output includes:
- ASCII or Mermaid diagram
- Property definitions
- Relationship descriptions
- Cardinality and constraints

### Identify Configuration Issues

Ask about potential issues in the DbContext:

> Check for any orphaned or circular relationships in the data model

The skill will flag:
- Missing foreign key constraints
- Potentially problematic cascade deletes
- Entities not included in DbSet
- Inconsistent naming or pattern violations

## Project Context

**DbContext Location**: `Marechai.Database/Data/MarechaiDbContext.cs`
**Models Location**: `Marechai.Database/Data/Models/`
**Configuration Location**: `Marechai.Database/Data/Configuration/`
**Database**: MySQL/MariaDB via Pomelo.EntityFrameworkCore.MySql (EF Core 9.0.11)

**Key Patterns in This Project**:
- Uses `IEntityTypeConfiguration<T>` for entity configurations
- Supports soft delete tracking (look for `IsDeleted` or similar fields)
- May use shadow properties for timestamps or audit fields
- MySQL-specific configurations for JSON columns (`Json.Microsoft`)

## Common Queries

### "What entities does Person relate to?"
The skill will trace all navigation properties from the Person entity and show cardinality.

### "Are there any unused DbSets?"
Checks all DbSet properties against references in migrations and services.

### "What's the relationship between X and Y?"
Identifies the specific foreign key, navigation properties, and delete behavior.

### "Show me the full model for a domain area"
Generates comprehensive documentation including all related entities and their interactions.

### "What are the cascade delete settings?"
Lists all relationships and their `OnDelete` behavior (Cascade, SetNull, Restrict, etc.).

## DbContext Analysis Output Format

When analyzing, the skill typically provides:

```
Entity: Computer
├─ Properties:
│  ├─ Id: int (PK)
│  ├─ Name: string
│  └─ ReleaseDate: DateTime?
├─ Navigation Properties:
│  ├─ Manufacturer: Manufacturer (FK: ManufacturerId)
│  ├─ Computers: List<Computer> (inverse)
├─ Configurations:
│  └─ HasIndex(c => c.Name)
└─ Related Entities: [Manufacturer, ComputerVariant, Software]
```

## Examples

**Analyze a relationship:**
```
/ef-dbcontext-analyzer What is the relationship between Software and SoftwareGenre? Show the navigation properties and foreign key
```

**Document a domain:**
```
/ef-dbcontext-analyzer Create ER documentation for the Person, Company, and Contact domain
```

**Find configuration issues:**
```
/ef-dbcontext-analyzer Check for any orphaned entities (DbSet properties that aren't referenced elsewhere)
```

**Understand cascade behavior:**
```
/ef-dbcontext-analyzer Show all delete cascade configurations - which entities will be deleted if their parent is removed
```

**Model evolution:**
```
/ef-dbcontext-analyzer Compare the Software entity between two migration points and show what changed
```

## Common Findings

This skill often identifies:

- **Missing Navigation Properties**: Relationships defined via foreign keys but not exposed as navigation properties
- **Asymmetric Relationships**: One side has a navigation property but the other doesn't
- **Naming Inconsistencies**: Foreign key names that don't match naming conventions
- **Unused Configurations**: Fluent API configurations that duplicate Annotations
- **Performance Issues**: Potentially missing indexes on frequently queried fields

## Integration with Other Skills

Works well with:
- **dotnet-migration-generator**: Understanding the current model before proposing schema changes
- **API Documentation**: DbContext structure informs API DTO design
- **Testing**: Entity relationships inform test data setup

## Commands Used

```bash
# List all entities in DbContext
dotnet ef dbcontext info --project Marechai.Database

# Generate script from current model
dotnet ef dbcontext scaffold-async --project Marechai.Database

# Check pending changes
dotnet build Marechai.Database.csproj
```
