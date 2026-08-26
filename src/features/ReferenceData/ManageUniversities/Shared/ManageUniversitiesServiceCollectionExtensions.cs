using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

public static class ManageUniversitiesServiceCollectionExtensions
{
  public static IServiceCollection AddManageUniversitiesPersistence(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection")
                           ?? Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION")
                           ?? throw new InvalidOperationException(
                             "No connection string found for Manage Universities persistence. " +
                             "Configure ConnectionStrings:DefaultConnection in appsettings.json or set ZEUS_SQLSERVER_CONNECTION environment variable.");

    services.AddDbContext<ManageUniversitiesDbContext>(options =>
      options.UseSqlServer(connectionString));

    return services;
  }

  public static IServiceCollection AddManageUniversitiesMediatR(
    this IServiceCollection services)
  {
    services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(typeof(ManageUniversitiesDbContext).Assembly));

    return services;
  }
}
