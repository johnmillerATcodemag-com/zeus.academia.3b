using Microsoft.EntityFrameworkCore;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

/// <summary>
/// DbContext for ManageUniversities feature.
/// Owns the Universities table and related domain entities.
/// </summary>
public sealed class ManageUniversitiesDbContext : DbContext
{
  public ManageUniversitiesDbContext(DbContextOptions<ManageUniversitiesDbContext> options)
      : base(options)
  {
  }

  /// <summary>
  /// Universities table - maps UniversityRecord entities.
  /// Configuration applied from UniversityRecordConfiguration.
  /// </summary>
  public DbSet<UniversityRecord> Universities => Set<UniversityRecord>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManageUniversitiesDbContext).Assembly);
  }
}
