using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Persistence;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

/// <summary>
/// Feature-local DbContext for the extension pool.
/// This is the sole migration owner for the Extensions table and reuses the
/// Shared Kernel configuration semantics rather than defining a duplicate entity.
/// </summary>
public sealed class ProvisionExtensionDbContext : DbContext
{
  public ProvisionExtensionDbContext(DbContextOptions<ProvisionExtensionDbContext> options)
    : base(options)
  {
  }

  public DbSet<Extension> Extensions { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new ExtensionConfiguration());
  }
}
