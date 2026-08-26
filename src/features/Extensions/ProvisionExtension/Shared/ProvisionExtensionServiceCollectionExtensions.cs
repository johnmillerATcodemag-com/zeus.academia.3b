using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

/// <summary>
/// Service collection extensions for ProvisionExtension feature.
/// Registers persistence (DbContext) and MediatR handlers.
/// </summary>
public static class ProvisionExtensionServiceCollectionExtensions
{
  /// <summary>
  /// Adds ProvisionExtensionDbContext and configures SQL Server connection.
  /// This context owns migrations for the Extensions table.
  /// Called from Program.cs during service registration (Phase 1).
  /// </summary>
  public static IServiceCollection AddProvisionExtensionPersistence(
      this IServiceCollection services,
      string connectionString)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

    services.AddDbContext<ProvisionExtensionDbContext>(options =>
        options.UseSqlServer(connectionString));

    return services;
  }

  /// <summary>
  /// Registers MediatR handlers from the ProvisionExtension assembly.
  /// Discovers and registers all commands, queries, and handlers.
  /// </summary>
  public static IServiceCollection AddProvisionExtensionMediatR(
      this IServiceCollection services)
  {
    services.AddValidatorsFromAssembly(typeof(ProvisionExtensionDbContext).Assembly);

    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(ProvisionExtensionDbContext).Assembly));

    return services;
  }
}
