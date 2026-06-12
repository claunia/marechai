# Claude Code Setup for Marechai

This document describes Claude Code automation for the Marechai computing history catalog project.

## Project Overview

**Marechai** is a .NET 10.0 ASP.NET Core application for cataloging and displaying computing history artifacts. It consists of multiple interconnected projects:

- **Marechai.Server**: ASP.NET Core Web API backend with OpenAPI documentation
- **Marechai.Database**: Entity Framework Core data access layer with MySQL/MariaDB
- **Marechai.App**: Uno Platform cross-platform client (Android, iOS, WASM, Desktop)
- **Marechai** (legacy): Original Blazor Server web application
- Supporting projects: Email, Translation, MobyGames, WinWorld integrations

### Key Technologies

- **.NET 10.0** (prerelease, see `global.json`)
- **Entity Framework Core 9.x** with **Pomelo MySQL** provider
- **Uno Platform SDK 6.5.36** for cross-platform UI
- **ASP.NET Core Identity** + JWT Bearer authentication
- **Blazorise** + MudBlazor for web UI components
- **SkiaSharp** for graphics/image processing

### Database

- **MySQL 8.0+** or **MariaDB 10.5+**
- Migrations stored in `Marechai.Database/Migrations/`
- Schema defined via DbContext in `Marechai.Database/Data/MarechaiDbContext.cs`
- Models in `Marechai.Database/Data/Models/`

## Local Development Setup

### Prerequisites

```bash
# .NET 10.0 SDK (with prerelease enabled in global.json)
dotnet --version  # Should show 10.0.x

# Verify prerelease SDK is enabled
cat global.json

# MySQL/MariaDB running locally
mysql --version

# ImageMagick with HEIF/WebP/AVIF support (for image processing)
convert --version
```

### Initial Setup

1. **Clone repository**
   ```bash
   git clone https://github.com/claunia/marechai.git
   cd marechai
   ```

2. **Configure database connection**
   - Edit `Marechai/appsettings.json` - set `DefaultConnection` string
   - Edit `Marechai.Server/appsettings.json` - set database connection
   - Create database: `CREATE DATABASE marechai CHARACTER SET utf8mb4;`

3. **Apply migrations**
   ```bash
   cd Marechai.Database
   dotnet ef database update --project Marechai.Database
   ```

4. **Build solution**
   ```bash
   dotnet build Marechai.slnx
   ```

5. **Run web API**
   ```bash
   dotnet run --project Marechai.Server
   # API available at https://localhost:7000 or http://localhost:5000
   ```

### Running the Cross-Platform App (Uno)

```bash
# WebAssembly (browser)
dotnet run --project Marechai.App -c Debug -f net10.0-browser

# Desktop (Windows/Linux/macOS)
dotnet run --project Marechai.App -c Debug -f net10.0-windows

# Android (requires Android SDK)
dotnet run --project Marechai.App -c Debug -f net10.0-android
```

## Claude Code Skills

Claude Code includes two specialized skills for managing the Marechai data model:

### `/dotnet-migration-generator`

**Purpose**: Generate, validate, and document EF Core migrations

**When to use**:
- Adding new tables or columns to the database schema
- Modifying entity relationships
- Creating data migrations (seeding, transformations)
- Validating migration correctness before commit

**Examples**:
```
/dotnet-migration-generator Add a new UserPreference table with Name and Value properties
/dotnet-migration-generator Validate the last 3 migrations for correctness
/dotnet-migration-generator Document the migration for the new ComputerFamily feature
```

**Key Points**:
- Operates on `Marechai.Database` project
- Uses `dotnet ef migrations add` under the hood
- Validates migrations compile and don't conflict
- Follows semantic naming conventions
- Includes both Up() and Down() implementations

### `/ef-dbcontext-analyzer`

**Purpose**: Analyze and document Entity Framework Core structure

**When to use**:
- Understanding entity relationships
- Documenting the data model for new team members
- Identifying missing navigation properties or indexes
- Checking cascade delete configurations
- Validating DbContext consistency

**Examples**:
```
/ef-dbcontext-analyzer What is the relationship between Computer and Manufacturer?
/ef-dbcontext-analyzer Document the complete Software domain with all related entities
/ef-dbcontext-analyzer Check for orphaned entities or circular relationships
/ef-dbcontext-analyzer Show cascade delete configurations for all relationships
```

**Key Points**:
- Analyzes `Marechai.Database/Data/MarechaiDbContext.cs`
- Maps all DbSet properties and their configurations
- Documents navigation properties and foreign keys
- Identifies configuration issues and patterns

## Coding Conventions

### C# Style

- Follow `.editorconfig` rules (UTF-8, 2-space indent, 120 char line length)
- Use C# 12+ features as appropriate
- Use PascalCase for public members, camelCase for private

### Naming

- **ViewModels**: `{Feature}ViewModel` (e.g., `ComputersListViewModel`)
- **Services**: `{Feature}Service` (e.g., `ComputersService`)
- **Pages/Views**: `{Feature}Page` (e.g., `ComputersListPage`)
- **Controllers**: `{Feature}Controller` (e.g., `ComputersController`)
- **Migrations**: `{YYYYMMDD}_{SemanticName}` (e.g., `20250612_AddUserPreferencesTable`)

### Architecture

- **Marechai.App**: MVVM with ViewModels in `Presentation/ViewModels/`, Views in `Presentation/Views/`
- **Marechai.Server**: Controller-based Web API with services in `Services/`
- **Marechai.Database**: Repository pattern with operations in `Operations/`
- **Dependency Injection**: Register in `App.xaml.cs` (Uno) or `Program.cs` (ASP.NET)

## Git Workflow

### Commits

**Claude must never create a commit on its own.** Only commit when the user
explicitly asks for a commit in that turn — never as a follow-up step after
finishing a task, fix, or feature, even if it seems like the natural next
action.

**All commits must be cryptographically signed** (see `CONTRIBUTING.md`).

```bash
# Configure signing (one-time)
git config user.signingkey <your-key-id>

# Sign commits by default
git config commit.gpgsign true

# Commit with signing (automatic if configured)
git commit -m "feat: Add user preferences feature"
```

### Branch Naming

- Feature: `feature/description`
- Bug fix: `fix/issue-number-description`
- Refactor: `refactor/description`
- Docs: `docs/description`

**Example**: `feature/add-user-preferences`

### Pull Requests

1. Keep changes focused and easy to review
2. Update documentation when behavior or setup changes
3. Validate locally before opening PR:
   ```bash
   dotnet build Marechai.slnx
   dotnet test
   ```
4. Write clear PR description explaining what and why

## Common Tasks

### Add a Database Field

```bash
# 1. Edit the entity model
# File: Marechai.Database/Data/Models/ComputerModel.cs
# Add property: public string? NewField { get; set; }

# 2. Generate migration
/dotnet-migration-generator Add NewField to Computer entity

# 3. Review generated migration in Marechai.Database/Migrations/

# 4. Apply locally
dotnet ef database update --project Marechai.Database
```

### Create a New API Endpoint

1. Add method to `Marechai.Server/Controllers/{Feature}Controller.cs`
2. Include OpenAPI documentation (XML comments)
3. Use dependency injection for services
4. Return appropriate HTTP status codes

### Add a New Uno Page

1. Create ViewModel: `Marechai.App/Presentation/ViewModels/{Feature}ViewModel.cs`
2. Create View: `Marechai.App/Presentation/Views/{Feature}Page.xaml`
3. Register in `App.xaml.cs`:
   ```csharp
   .AddView<{Feature}Page, {Feature}ViewModel>()
   ```
4. Add route to `RouteMap` in `App.xaml.cs`

### Understand Data Relationships

```
/ef-dbcontext-analyzer Show the relationship between {Entity1} and {Entity2}
```

This will show navigation properties, foreign keys, cardinality, and delete behavior.

## Important Files

- `Marechai.slnx` - Modern solution file (use this, not `.sln`)
- `Directory.Build.props` - Shared MSBuild properties
- `Directory.Packages.props` - Central NuGet package version management
- `global.json` - SDK version and prerelease settings
- `.editorconfig` - Code style rules for all languages
- `AGENTS.md` - Detailed architecture and patterns reference
- `CONTRIBUTING.md` - Contribution guidelines and signing instructions

## MCP Servers

Two MCP servers are recommended for this project:

1. **GitHub MCP** (`claude mcp add github`)
   - View and manage PRs, issues, and actions
   - Review code directly from Claude

2. **Microsoft Docs MCP** (built-in)
   - Look up .NET, ASP.NET Core, EF Core documentation
   - Search Microsoft Learn for API references

## Troubleshooting

### Migrations Won't Apply

```bash
# Check for pending migrations
dotnet ef migrations list --project Marechai.Database

# Verify database connection
dotnet ef dbcontext info --project Marechai.Database

# Revert last migration (if needed)
dotnet ef migrations remove --project Marechai.Database
```

### Build Fails

```bash
# Clean build
dotnet clean Marechai.slnx
dotnet build Marechai.slnx

# Check SDK version
dotnet --version

# Verify dependencies
dotnet restore
```

### Database Connection Issues

- Check `appsettings.json` connection string format
- Verify MySQL/MariaDB is running: `mysql -u root -p`
- Test connection: `dotnet ef dbcontext info --project Marechai.Database`

## Resources

- **AGENTS.md** - Detailed architecture patterns and guidelines
- **CONTRIBUTING.md** - Contribution workflow and commit signing
- **GitHub**: https://github.com/claunia/marechai
- **Microsoft Learn**: https://learn.microsoft.com/en-us/dotnet/
- **Entity Framework Core**: https://learn.microsoft.com/en-us/ef/core/
- **Uno Platform**: https://platform.uno/

---

**Last updated**: June 2026
**For Claude Code assistance**: `/help` or https://github.com/anthropics/claude-code/issues
