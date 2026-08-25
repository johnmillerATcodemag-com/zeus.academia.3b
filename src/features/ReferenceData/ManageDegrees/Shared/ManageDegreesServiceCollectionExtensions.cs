using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Zeus.Academia.Features.ReferenceData.ManageDegrees.Shared;

/// <summary>
/// Service collection extensions for Manage Degrees persistence and handlers registration.
/// Used by the application host to register the ManageDegreesDbContext, MediatR handlers, and validators.
/// </summary>
public static class ManageDegreesServiceCollectionExtensions
{
  /// <summary>
  /// Registers the ManageDegreesDbContext with the service collection.
  /// The connection string is resolved from configuration with environment-based fallback.
  /// </summary>
  /// <param name="services">The service collection to register services into.</param>
  /// <param name="configuration">The application configuration containing connection strings.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddManageDegreesPersistence(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                          Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION") ??
                          throw new InvalidOperationException(
                            "No connection string found for Manage Degrees persistence. " +
                            "Configure ConnectionStrings:DefaultConnection in appsettings.json or set ZEUS_SQLSERVER_CONNECTION environment variable.");

    services.AddDbContext<ManageDegreesDbContext>(options =>
      options.UseSqlServer(connectionString));

    return services;
  }

  /// <summary>
  /// Registers MediatR handlers and validators for Manage Degrees feature.
  /// Scans the ManageDegreesDbContext assembly for all handler implementations and validators.
  /// </summary>
  /// <param name="services">The service collection to register services into.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddManageDegreesMediatR(
    this IServiceCollection services)
  {
    services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(typeof(ManageDegreesDbContext).Assembly));

    return services;
  }
}
