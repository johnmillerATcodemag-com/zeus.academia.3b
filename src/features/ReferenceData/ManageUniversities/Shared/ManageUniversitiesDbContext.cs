using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

public sealed class ManageUniversitiesDbContext : DbContext
{
  public ManageUniversitiesDbContext(DbContextOptions<ManageUniversitiesDbContext> options)
    : base(options)
  {
  }

  public DbSet<UniversityRecord> Universities => Set<UniversityRecord>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManageUniversitiesDbContext).Assembly);
  }
}
