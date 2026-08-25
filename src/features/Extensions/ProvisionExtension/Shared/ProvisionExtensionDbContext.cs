using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Persistence;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

/// <summary>
/// DbContext for ProvisionExtension feature.
/// CRITICAL: This context is the sole migration owner for the Extensions table.
/// Reuses the Shared Kernel Extension entity and its configuration to avoid duplication.
/// </summary>
public class ProvisionExtensionDbContext : DbContext
{
  public ProvisionExtensionDbContext(DbContextOptions<ProvisionExtensionDbContext> options)
      : base(options)
  {
  }

  /// <summary>
  /// Extensions table - maps Extension entities from Shared Kernel.
  /// ProvisionExtensionDbContext owns the migration path for this table.
  /// </summary>
  public DbSet<Extension> Extensions { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Apply Extension configuration from Shared Kernel
    // This reuses the same schema configuration without duplicating the entity definition
    modelBuilder.ApplyConfiguration(new ExtensionConfiguration());
  }
}
