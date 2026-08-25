using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.SharedKernel.Foundation.Persistence;

public sealed class SharedKernelDbContext : DbContext
{
  public SharedKernelDbContext(DbContextOptions<SharedKernelDbContext> options)
    : base(options)
  {
  }

  public DbSet<Academic> Academics => Set<Academic>();

  public DbSet<AcademicQualification> AcademicQualifications => Set<AcademicQualification>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Explicitly apply only configurations for tables owned by SharedKernelDbContext
    // Extensions configuration is excluded because ProvisionExtensionDbContext is the sole migration owner
    modelBuilder.ApplyConfiguration(new AcademicConfiguration());
    modelBuilder.ApplyConfiguration(new AcademicQualificationConfiguration());
  }
}
