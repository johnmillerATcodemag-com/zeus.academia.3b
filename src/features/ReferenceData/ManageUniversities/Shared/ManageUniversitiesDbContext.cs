using Microsoft.EntityFrameworkCore;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

/// <summary>
/// DbContext for ManageUniversities feature.
/// Owns the Universities table and related domain entities.
/// </summary>
public class ManageUniversitiesDbContext : DbContext
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
