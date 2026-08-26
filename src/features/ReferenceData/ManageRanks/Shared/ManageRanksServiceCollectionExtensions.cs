using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Zeus.Academia.Features.ReferenceData.ManageRanks.Shared;

/// <summary>
/// Service collection extensions for Manage Ranks persistence and handlers registration.
/// Used by the application host to register the ManageRanksDbContext, MediatR handlers, and validators.
/// </summary>
public static class ManageRanksServiceCollectionExtensions
{
  /// <summary>
  /// Registers the ManageRanksDbContext with the service collection.
  /// The connection string is resolved by the application host and shared by all contexts.
  /// </summary>
  /// <param name="services">The service collection to register services into.</param>
  /// <param name="connectionString">The SQL Server connection string resolved by the application host.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddManageRanksPersistence(
    this IServiceCollection services,
    string connectionString)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

    services.AddDbContext<ManageRanksDbContext>(options =>
      options.UseSqlServer(connectionString));

    return services;
  }

  /// <summary>
  /// Registers MediatR handlers and validators for Manage Ranks feature.
  /// Scans the ManageRanksDbContext assembly for all handler implementations and validators.
  /// </summary>
  /// <param name="services">The service collection to register services into.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddManageRanksMediatR(
    this IServiceCollection services)
  {
    services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(typeof(ManageRanksDbContext).Assembly));

    return services;
  }
}
