using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

/// <summary>
/// Service collection extensions for the extension provisioning feature.
/// </summary>
public static class ProvisionExtensionServiceCollectionExtensions
{
  public static IServiceCollection AddProvisionExtensionPersistence(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    services.AddDbContext<ProvisionExtensionDbContext>(options =>
        options.UseSqlServer(connectionString));

    return services;
  }

  public static IServiceCollection AddProvisionExtensionMediatR(
      this IServiceCollection services)
  {
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(ProvisionExtensionDbContext).Assembly));

    return services;
  }
}
