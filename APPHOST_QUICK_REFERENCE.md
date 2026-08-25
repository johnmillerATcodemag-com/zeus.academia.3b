---
title: "Application Host Quick Reference"
phase: "Phase 0 - Step 2"
purpose: "Developer guide for the Application Host and Composition Root"
audience: "Backend developers implementing features in Phase 1+"
---

# Application Host: Quick Reference Guide

## Project Location

- **Path**: `src/Zeus.Academia.Api/`
- **Project Name**: `Zeus.Academia.Api`
- **Type**: ASP.NET Core Web API (Minimal API)
- **Framework**: .NET 8.0

## Key Files

| File                           | Purpose                                                     |
| ------------------------------ | ----------------------------------------------------------- |
| `Program.cs`                   | Composition root, DI configuration, migration orchestration |
| `Zeus.Academia.Api.csproj`     | Project definition with all package and project references  |
| `appsettings.json`             | Connection string and base logging configuration            |
| `appsettings.Development.json` | Development overrides for detailed logging                  |
| `Endpoints/`                   | Folder for route definitions (added by feature projects)    |

## Connection String Configuration

### Resolution Order (in Program.cs)

```
1. Environment Variable: ZEUS_SQLSERVER_CONNECTION
2. Config File: ConnectionStrings:DefaultConnection (appsettings.json)
3. Windows LocalDB: (localdb)\mssqllocaldb (Windows only)
4. Non-Windows: Throws InvalidOperationException
```

### Set Connection String (Non-Windows CI/CD)

```bash
export ZEUS_SQLSERVER_CONNECTION="Server=yourserver;Database=ZeusAcademia;User Id=user;Password=pass;"
dotnet run --project src/Zeus.Academia.Api/
```

## Registering a New Feature (Phase 1+)

### Step 1: Create ServiceCollectionExtensions in Feature

```csharp
// File: src/features/YourDomain/YourFeature/Shared/YourFeatureServiceCollectionExtensions.cs

public static class YourFeatureServiceCollectionExtensions
{
  public static IServiceCollection AddYourFeaturePersistence(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                          Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION") ??
                          throw new InvalidOperationException("No connection string found.");

    services.AddDbContext<YourFeatureDbContext>(options =>
      options.UseSqlServer(connectionString));

    return services;
  }

  public static IServiceCollection AddYourFeatureMediatR(this IServiceCollection services)
  {
    services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(typeof(YourFeatureDbContext).Assembly));

    return services;
  }
}
```

### Step 2: Register in Program.cs

```csharp
// In Program.cs, after existing feature registrations:

// Feature: Your Feature
builder.Services.AddYourFeaturePersistence(builder.Configuration);
builder.Services.AddYourFeatureMediatR();

// In migration orchestration section:
using (var scope = app.Services.CreateScope())
{
  var serviceProvider = scope.ServiceProvider;

  // ... existing migrations ...

  // Your Feature
  var yourFeatureContext = serviceProvider.GetRequiredService<YourFeatureDbContext>();
  await yourFeatureContext.Database.MigrateAsync();
}
```

### Step 3: Register Endpoints

```csharp
// In Program.cs, before app.Run():
// Features register their endpoints via MapGroup or similar
var yourFeatureGroup = app.MapGroup("/api/your-feature")
  .WithName("YourFeature")
  .WithOpenApi();

// Your endpoints go here
```

## Health Check Endpoint

**URL**: `GET /health`

**Response**:

```json
{
  "status": "healthy"
}
```

**Use Case**: Deployment verification, load balancer health checks

**Implementation**:

```csharp
// In Program.cs
app.MapHealthCheck("/health");

static class HealthCheckExtensions
{
  public static void MapHealthCheck(this WebApplication app, string pattern)
  {
    app.MapGet(pattern, () => Results.Ok(new { status = "healthy" }))
      .WithName("Health")
      .WithOpenApi();
  }
}
```

## Database Migrations

### List All Migrations

```bash
# Multiple DbContexts - must specify which one
dotnet ef migrations list -p src/Zeus.Academia.Api/ --context SharedKernelDbContext
dotnet ef migrations list -p src/Zeus.Academia.Api/ --context ManageRanksDbContext
dotnet ef migrations list -p src/Zeus.Academia.Api/ --context ManageDegreesDbContext
```

### Add a New Migration

```bash
# For Manage Ranks feature (example)
dotnet ef migrations add InitialCreate \
  -p src/features/ReferenceData/ManageRanks/Zeus.Academia.Features.ReferenceData.ManageRanks.csproj \
  --context ManageRanksDbContext \
  --output-dir Persistence/Migrations

# For a new Phase 1 feature
dotnet ef migrations add InitialCreate \
  -p src/features/YourDomain/YourFeature/Zeus.Academia.Features.YourDomain.YourFeature.csproj \
  --context YourFeatureDbContext \
  --output-dir Persistence/Migrations
```

### Apply Migrations

Migrations run automatically on application startup. To apply manually:

```bash
dotnet ef database update \
  -p src/Zeus.Academia.Api/ \
  --context ManageRanksDbContext
```

## Running the Application

### Local Development (Windows with LocalDB)

```bash
dotnet run --project src/Zeus.Academia.Api/

# Application will:
# 1. Parse connection string from appsettings.json
# 2. Fallback to LocalDB: (localdb)\mssqllocaldb
# 3. Create/use database: Zeus_Academia_Dev
# 4. Apply all pending migrations
# 5. Start listening on http://localhost:5000
```

### Local Development (Non-Windows)

```bash
# Set SQL Server connection (required - LocalDB not available)
export ZEUS_SQLSERVER_CONNECTION="Server=sqlserver;Database=ZeusAcademia;User Id=sa;Password=YourPassword;"

dotnet run --project src/Zeus.Academia.Api/
```

### Production

```bash
# Set connection via environment variable or secrets manager
export ZEUS_SQLSERVER_CONNECTION="Production connection string"

dotnet run --project src/Zeus.Academia.Api/ --configuration Release
```

## Common Tasks

### Verify Host Builds

```bash
dotnet build src/Zeus.Academia.Api/
```

### Check All DbContexts Available

```bash
# This should list all DbContexts available in the host
dotnet ef dbcontext list -p src/Zeus.Academia.Api/
```

### View Current Database Schema

```bash
# Generate SQL from current model
dotnet ef dbcontext optimize --assembly src/Zeus.Academia.Api/bin/Debug/net8.0/Zeus.Academia.Api.dll
```

### Test a Specific DbContext

```bash
# Verify SharedKernel context can connect
dotnet ef dbcontext info -p src/Zeus.Academia.Api/ --context SharedKernelDbContext
```

## Troubleshooting

### "Multiple DbContexts found" Error

**Cause**: Running EF command without specifying `--context`

**Solution**: Add `--context <ContextName>` parameter

```bash
dotnet ef migrations list -p src/Zeus.Academia.Api/ --context ManageRanksDbContext
```

### "No connection string found" Error

**Cause**: Non-Windows host without `ZEUS_SQLSERVER_CONNECTION` environment variable

**Solution**: Set environment variable before running

```bash
export ZEUS_SQLSERVER_CONNECTION="Server=...;Database=...;..."
```

### Migrations Table Not Found

**Cause**: Database connection issue

**Solution**: Verify:

1. Connection string is correct
2. Database server is accessible
3. User has CREATE DATABASE permission (for initial setup)

### Application Won't Start - Migration Fails

**Cause**: Schema mismatch or missing migration

**Solution**:

1. Check migration files exist in feature's `Persistence/Migrations/` folder
2. Verify Designer metadata is present
3. Run migration in isolation to see detailed error:
   ```bash
   dotnet ef database update -p src/Zeus.Academia.Api/ --context ManageRanksDbContext --verbose
   ```

## Key Patterns

### Extension Method Pattern

Each feature should follow this pattern:

```csharp
public static IServiceCollection AddXxxPersistence(
  this IServiceCollection services,
  IConfiguration configuration)
{
  // Register DbContext
  return services;
}

public static IServiceCollection AddXxxMediatR(
  this IServiceCollection services)
{
  // Register handlers and validators
  return services;
}
```

### Configuration Pattern

Always use this priority order:

1. Environment variable (override)
2. Configuration file (default)
3. Fallback with guard (Windows-only)
4. Exception (non-Windows without config)

### Migration Pattern

Each feature owns its migrations:

- Location: `src/features/Domain/Feature/Persistence/Migrations/`
- No two DbContexts migrate the same table
- Migrations auto-apply on host startup

## File Organization Reference

```
src/Zeus.Academia.Api/
├── Program.cs                    ← Composition root
├── Zeus.Academia.Api.csproj      ← Project definition
├── appsettings.json              ← Connection strings
├── appsettings.Development.json  ← Dev overrides
└── Endpoints/                    ← Route definitions (per feature)

src/features/SharedKernel/Foundation/
├── Persistence/
│   ├── SharedKernelDbContext.cs                     ← DbContext
│   ├── SharedKernelServiceCollectionExtensions.cs   ← DI registration
│   ├── Migrations/                                  ← Schema changes
│   └── [Configuration files for entities]

src/features/ReferenceData/ManageRanks/
├── AddRank/
│   ├── AddRankCommand.cs
│   ├── AddRankCommandValidator.cs
│   └── AddRankHandler.cs
├── ListRanks/
├── Shared/
│   ├── ManageRanksDbContext.cs
│   └── ManageRanksServiceCollectionExtensions.cs
└── ManageRanksEndpoints.cs       ← Endpoint definitions
```

## Summary

The Application Host is the entry point for the zeus.academia system. It:

- **Orchestrates** service registration and dependency injection
- **Configures** database connections with fallback logic
- **Manages** database migrations on startup
- **Provides** minimal API infrastructure for route definitions
- **Supports** extensibility for new features in Phase 1+

All features follow a consistent pattern for registration, making it easy to onboard new domain features as the system grows.

---

**Last Updated**: 2026-08-24
**For Questions**: Refer to APPLICATION_HOST_IMPLEMENTATION.md and IMPLEMENTATION_VERIFICATION.md
