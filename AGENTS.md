# AI Agent Guidelines for Marechai

This document provides guidance for AI coding agents working with the Marechai codebase.

## Project Overview

Marechai (Master repository of computing history artifacts information) is a .NET solution for cataloging and displaying computing history artifacts. The solution consists of multiple projects targeting .NET 10.0.

## Solution Structure

```
Marechai.slnx
├── Marechai/              # Blazor Server web application (legacy)
├── Marechai.App/          # Uno Platform cross-platform client app
├── Marechai.Server/       # ASP.NET Core Web API backend
├── Marechai.Database/     # Entity Framework Core data access layer
└── Marechai.Data/         # Shared DTOs and data models
```

### Project Details

- **Marechai** (`Marechai.csproj`): Blazor Server web application using ASP.NET Core with Identity, Bootstrap via Blazorise, and Entity Framework Core with MySQL/MariaDB.

- **Marechai.App** (`Marechai.App.csproj`): Uno Platform cross-platform application targeting Android, iOS, WebAssembly (WASM), and Desktop. Uses MVVM pattern with Uno.Extensions for navigation, hosting, HTTP (Kiota), localization, and theming. Employs SkiaSharp for rendering.

- **Marechai.Server** (`Marechai.Server.csproj`): ASP.NET Core Web API backend providing RESTful endpoints. Uses JWT Bearer authentication, OpenAPI documentation, and Entity Framework Core.

- **Marechai.Database** (`Marechai.Database.csproj`): Data access layer using Entity Framework Core with Pomelo MySQL provider. Contains database models, migrations, seeders, and operations.

- **Marechai.Data** (`Marechai.Data.csproj`): Shared library containing DTOs (Data Transfer Objects) and enums used across projects.

## Technology Stack

- **.NET 10.0** (Preview/Prerelease)
- **Uno Platform SDK 6.4.24** for cross-platform development
- **Entity Framework Core 9.x** with Pomelo MySQL provider
- **ASP.NET Core Identity** for authentication
- **JWT Bearer Authentication** for API security
- **Blazorise Bootstrap** for Blazor UI components
- **SkiaSharp** for graphics/image processing
- **Kiota** for HTTP client generation
- **MariaDB/MySQL** as the database backend

## Build and Run

### Prerequisites

- .NET 10.0 SDK (with prerelease allowed per `global.json`)
- Uno Platform SDK 6.4.24
- MariaDB or MySQL database
- ImageMagick with HEIF, WebP and AVIF support (in PATH)

### Configuration

- Configure `appsettings.json` with database connection string
- API URL configured in `Marechai.App/appsettings.json` under `ApiClient:Url`

## Coding Conventions

### General

- Use C# 12+ features as appropriate for .NET 10.0
- Central package management via `Directory.Packages.props`
- Follow existing code style (see `.editorconfig`)
- All commits must be cryptographically signed (see `CONTRIBUTING.md`)

### Architecture Patterns

- **Marechai.App**: MVVM pattern with ViewModels in `Presentation/ViewModels/` and Views in `Presentation/Views/`
- **Marechai.Server**: Controller-based Web API with services in `Services/`
- **Marechai.Database**: Repository pattern with operations in `Operations/`

### Naming Conventions

- ViewModels: `{Feature}ViewModel` (e.g., `ComputersListViewModel`)
- Services: `{Feature}Service` (e.g., `ComputersService`)
- Pages/Views: `{Feature}Page` (e.g., `ComputersListPage`)
- Controllers: `{Feature}Controller`

### Dependency Injection

Services and ViewModels are registered in `App.xaml.cs` for Marechai.App:
- Use `AddSingleton<>` for shared state services and caches
- Use `AddTransient<>` for ViewModels that should be recreated per navigation

### Navigation (Marechai.App)

Routes are registered in `App.xaml.cs` using Uno.Extensions.Navigation:
- Views are mapped to ViewModels via `ViewMap<TView, TViewModel>`
- Routes are hierarchical using nested `RouteMap` definitions

## Important Files

- `Directory.Build.props` - Common MSBuild properties
- `Directory.Packages.props` - Centralized NuGet package versions
- `global.json` - SDK version configuration
- `CONTRIBUTING.md` - Contribution guidelines (commit signing required)
- `Marechai.App/App.xaml.cs` - App startup, DI registration, and route configuration

## Testing

Test results are stored in the `TestResults/` directory.

## Database

- Uses MariaDB/MySQL
- Migrations located in `Marechai.Database/Migrations/`
- Seeders in `Marechai.Database/Seeders/`
- Database schemas in `Marechai.Database/Schemas/`

## Notes for AI Agents

0. Persistent agent notes and lessons learned should be stored under `.agents/` in this repository.

1. When adding new pages/views to Marechai.App:
   - Create ViewModel in `Presentation/ViewModels/`
   - Create View (XAML) in `Presentation/Views/`
   - Register service/ViewModel in `App.xaml.cs`
   - Add route mapping in `RegisterRoutes` method

2. When adding new API endpoints:
   - Add controller in `Marechai.Server/Controllers/`
   - Add corresponding service if needed
   - Update DTOs in `Marechai.Data/Dtos/` as needed

3. When modifying database:
   - Update models in `Marechai.Database/Models/`
   - Generate migrations using EF Core tools
   - Update DTOs in `Marechai.Data/` if needed

4. Package versions are centrally managed - update `Directory.Packages.props` when adding/updating packages.

5. The project targets .NET 10.0 preview - be aware of potential API changes.
