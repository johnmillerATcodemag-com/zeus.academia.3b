using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    var connectionString = configuration.GetConnectionString("DefaultConnection");

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

    return services;
  }
}
