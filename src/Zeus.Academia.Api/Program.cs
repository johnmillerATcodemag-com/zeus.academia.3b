using Microsoft.EntityFrameworkCore;
using MediatR;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.ReferenceData.ManageDegrees;
using Zeus.Academia.Features.ReferenceData.ManageDegrees.Shared;
using Zeus.Academia.Features.ReferenceData.ManageRanks;
using Zeus.Academia.Features.ReferenceData.ManageRanks.Shared;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.SharedKernel.Foundation.Persistence;

// ============================================================================
// Application Host: Composition Root and Migration Orchestration
// ============================================================================
// This host orchestrates:
// 1. Service registration (persistence, MediatR, validators)
// 2. Configuration management (connection strings, environment settings)
// 3. Database migration execution on startup
// 4. Minimal API route configuration (route definitions in feature projects)
//
// Registration Order (MUST maintain dependency chain):
// - SharedKernelDbContext (prerequisite for all features)
// - ManageRanksDbContext (independent, Phase 0)
// - ManageDegreesDbContext (independent, Phase 0)
// - MediatR handlers and validators for each feature
//
// Phase 1 features (e.g., ProvisionExtension) will be registered by their
// respective agents when added to the slicing plan.
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Connection String Configuration
// ============================================================================
// Priority:
// 1. Environment variable: ZEUS_SQLSERVER_CONNECTION (CI/CD, non-Windows)
// 2. Configuration: ConnectionStrings:DefaultConnection (appsettings.json)
// 3. Windows LocalDB fallback (development on Windows only)

var connectionString = Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION");

if (string.IsNullOrWhiteSpace(connectionString))
{
  connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
  if (OperatingSystem.IsWindows())
  {
    // Windows LocalDB fallback for local development only.
    // Use the default LocalDB instance name, which is MSSQLLocalDB on standard Windows installs.
    connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Zeus_Academia_Dev;Integrated Security=True;TrustServerCertificate=True;";
  }
  else
  {
    // Non-Windows platforms require explicit SQL Server connection
    throw new InvalidOperationException(
      "SQL Server connection string not found. Set ZEUS_SQLSERVER_CONNECTION environment variable or add ConnectionStrings:DefaultConnection to appsettings.json. " +
      "LocalDB is only available on Windows; configure a SQL Server connection string for non-Windows environments.");
  }
}

// ============================================================================
// Service Registration: Persistence (DbContexts)
// ============================================================================
// Registration order matters: SharedKernel must be registered before features
// that depend on it.
// NOTE: Connection string resolution is a host concern, not feature concern.
// Features receive the resolved connection string, not IConfiguration.

// Shared Kernel DbContext (prerequisite for all features)
builder.Services.AddDbContext<SharedKernelDbContext>(options =>
  options.UseSqlServer(connectionString));

// Feature 1: Manage Ranks (Phase 0)
builder.Services.AddManageRanksPersistence(builder.Configuration);
builder.Services.AddManageRanksMediatR();

// Feature 2: Manage Degrees (Phase 0)
builder.Services.AddManageDegreesPersistence(builder.Configuration);
builder.Services.AddManageDegreesMediatR();

// Feature 3: Manage Universities (Phase 1 ownership path)
builder.Services.AddManageUniversitiesPersistence(builder.Configuration);
builder.Services.AddManageUniversitiesMediatR();

// Feature 4: Provision Extension (sole migration owner for Extensions)
builder.Services.AddProvisionExtensionPersistence(builder.Configuration);
builder.Services.AddProvisionExtensionMediatR();

// ============================================================================
// Service Registration: MediatR Handlers and Validators
// ============================================================================
// MediatR registration for each feature's command/query handlers and validators.
// NOTE: MediatR registration is a host concern (discovers handlers by assembly).
// Features should NOT duplicate this logic; the host orchestrates handler discovery.

// Shared Kernel handlers
builder.Services.AddMediatR(cfg =>
  cfg.RegisterServicesFromAssembly(typeof(SharedKernelDbContext).Assembly));

// ============================================================================
// Application Configuration
// ============================================================================

var app = builder.Build();

// ============================================================================
// Database Migration Orchestration
// ============================================================================
// Automatically apply pending migrations for all registered DbContexts on
// startup. Failures are NOT suppressed; they indicate configuration or
// dependency issues that must be resolved before the app can run.

using (var scope = app.Services.CreateScope())
{
  var serviceProvider = scope.ServiceProvider;

  // Migration order (matches registration order):
  // 1. Shared Kernel (prerequisite)
  var sharedKernelContext = serviceProvider.GetRequiredService<SharedKernelDbContext>();
  await sharedKernelContext.Database.MigrateAsync();

  // 2. Manage Ranks
  var manageRanksContext = serviceProvider.GetRequiredService<ManageRanksDbContext>();
  await manageRanksContext.Database.MigrateAsync();

  // 3. Manage Degrees
  var manageDegreesContext = serviceProvider.GetRequiredService<ManageDegreesDbContext>();
  await manageDegreesContext.Database.MigrateAsync();

  // 4. Manage Universities
  var manageUniversitiesContext = serviceProvider.GetRequiredService<ManageUniversitiesDbContext>();
  await manageUniversitiesContext.Database.MigrateAsync();

  // 5. Provision Extension (sole migration owner for Extensions)
  var provisionExtensionContext = serviceProvider.GetRequiredService<ProvisionExtensionDbContext>();
  await provisionExtensionContext.Database.MigrateAsync();
}

// ============================================================================
// Minimal API Route Configuration
// ============================================================================
// Route definitions are owned by feature projects. The host only provides
// the WebApplication instance and shared middleware/endpoints.

app.MapManageDegreesEndpoints();
app.MapManageRanksEndpoints();
app.MapHealthCheck("/health");

app.Run();

// ============================================================================
// Health Check Endpoint
// ============================================================================
// Minimal smoke test endpoint for deployment verification.
static class HealthCheckExtensions
{
  public static void MapHealthCheck(this WebApplication app, string pattern)
  {
    app.MapGet(pattern, () => Results.Ok(new { status = "healthy" }))
      .WithName("Health")
      .WithOpenApi();
  }
}
