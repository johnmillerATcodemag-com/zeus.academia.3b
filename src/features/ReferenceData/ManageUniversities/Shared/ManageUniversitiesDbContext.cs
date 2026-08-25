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

  /// <summary>
  /// Universities table - maps UniversityRecord entities.
  /// Configuration applied from UniversityRecordConfiguration (Phase 1).
  /// </summary>
  public DbSet<UniversityRecord> Universities { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Apply all entity configurations from this assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManageUniversitiesDbContext).Assembly);
  }
}

/// <summary>
/// Placeholder for UniversityRecord entity.
/// Full implementation and configuration come with EP-1-3.
/// </summary>
public class UniversityRecord
{
  // Placeholder - domain model defined in EP-1-3
}
