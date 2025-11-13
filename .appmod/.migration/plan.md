# .NET Modernization Plan: Local MariaDB Application

## Overview
Complete modernization of Marechai application from .NET 5 to .NET 9 with dependency updates and local credential security.

## Goals
1. Upgrade from .NET 5.0 to .NET 9.0
2. Update all NuGet packages to latest compatible versions
3. Fix security vulnerabilities (SkiaSharp CVE)
4. Remove deprecated dependencies (Application Insights)
5. Implement secure local credential storage using Data Protection API
6. Maintain all MariaDB functionality and local-only architecture

## Projects
- **Marechai.Database.csproj** - Class library (net5.0 → net9.0)
- **Marechai.csproj** - ASP.NET Core app (net5.0 → net9.0)

## Migration Tasks

### Task 1: Upgrade .NET Framework
- Update Marechai.Database.csproj target framework from net5.0 to net9.0
- Update Marechai.csproj target framework from net5.0 to net9.0
- Verify framework migration compatibility

### Task 2: Update Critical NuGet Packages

#### Marechai.Database.csproj
- Microsoft.AspNetCore.Identity.EntityFrameworkCore: 5.0.1 → 9.0.11
- Microsoft.EntityFrameworkCore.Design: 5.0.1 → 9.0.11
- Microsoft.EntityFrameworkCore.Proxies: 5.0.1 → 9.0.11

#### Marechai.csproj
- Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore: 5.0.1 → 9.0.11
- Microsoft.AspNetCore.Identity.EntityFrameworkCore: 5.0.1 → 9.0.11
- Microsoft.AspNetCore.Identity.UI: 5.0.1 → 9.0.11
- Microsoft.EntityFrameworkCore.Proxies: 5.0.1 → 9.0.11
- Microsoft.EntityFrameworkCore.Tools: 5.0.1 → 9.0.11
- Microsoft.VisualStudio.Web.CodeGeneration.Design: 5.0.1 → 9.0.0
- SkiaSharp: 2.80.2 → 3.119.1
- SkiaSharp.NativeAssets.Linux: 2.80.2 → 3.119.1

### Task 3: Remove Deprecated Packages
- Microsoft.ApplicationInsights.AspNetCore: 2.16.0 (deprecated)

### Task 4: Implement Local Credential Encryption
- Add Data Protection API configuration in appsettings.json
- Create credential encryption utility for DPAPI integration
- Update connection string handling in Program.cs
- Add encrypted credential support in Startup.cs
- Ensure backward compatibility with existing deployments

## Execution Order
1. Upgrade .NET framework versions
2. Update NuGet packages
3. Fix security vulnerabilities
4. Remove deprecated dependencies
5. Implement credential encryption
6. Comprehensive testing and validation
7. Commit all changes

## Success Criteria
- ✅ Both projects target .NET 9.0
- ✅ All recommended packages updated
- ✅ SkiaSharp security vulnerability fixed
- ✅ Application Insights deprecated package removed
- ✅ Credentials can be securely stored locally
- ✅ Build succeeds with no errors
- ✅ All changes committed to git
