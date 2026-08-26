using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

/// <summary>
/// Service collection extensions for ManageUniversities feature.
/// Registers persistence (DbContext) and MediatR handlers.
/// </summary>
public static class ManageUniversitiesServiceCollectionExtensions
{
  /// <summary>
  /// Adds ManageUniversitiesDbContext and configures SQL Server connection.
  /// Called from Program.cs during service registration (Phase 1).
  /// </summary>
  public static IServiceCollection AddManageUniversitiesPersistence(
      this IServiceCollection services,
      IConfiguration configuration)
  {
var connectionString = Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION");

if (string.IsNullOrWhiteSpace(connectionString))
{
  connectionString = configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
  if (OperatingSystem.IsWindows())
  {
    connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Zeus_Academia_Dev;Integrated Security=True;TrustServerCertificate=True;";
  }
  else
  {
    throw new InvalidOperationException(
      "SQL Server connection string not found. Set ZEUS_SQLSERVER_CONNECTION environment variable or add ConnectionStrings:DefaultConnection to appsettings.json. " +
      "LocalDB is only available on Windows; configure a SQL Server connection string for non-Windows environments.");
  }
}

    services.AddDbContext<ManageUniversitiesDbContext>(options =>
        options.UseSqlServer(connectionString));

    return services;
  }

  /// <summary>
  /// Registers MediatR handlers from the ManageUniversities assembly.
  /// Discovers and registers all commands, queries, and handlers.
  /// </summary>
  public static IServiceCollection AddManageUniversitiesMediatR(
      this IServiceCollection services)
  {
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(ManageUniversitiesDbContext).Assembly));

    services.AddScoped<IValidator<AddUniversityCommand>, AddUniversityCommandValidator>();

    return services;
  }
}
