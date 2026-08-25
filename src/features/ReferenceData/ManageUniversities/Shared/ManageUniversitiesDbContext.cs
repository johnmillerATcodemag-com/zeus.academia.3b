using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

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
  /// Configuration applied from UniversityRecordConfiguration.
  /// </summary>
  public DbSet<UniversityRecord> Universities { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManageUniversitiesDbContext).Assembly);
  }
}

/// <summary>
/// Represents an institution catalog entry keyed by code.
/// The code is the catalog's primary key and maps to the Shared Kernel University value object code.
/// </summary>
public sealed class UniversityRecord
{
  public string Code { get; private set; } = string.Empty;

  public string Name { get; private set; } = string.Empty;

  public bool IsActive { get; private set; } = true;

  public static UniversityRecord Create(string code, string name)
  {
    if (string.IsNullOrWhiteSpace(code))
      throw new ArgumentException("University code is required.", nameof(code));

    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("University name is required.", nameof(name));

    var normalizedCode = code.Trim();
    var normalizedName = name.Trim();

    if (normalizedCode.Length > SharedKernelFieldLengths.UniversityCode)
      throw new ArgumentException($"University code cannot exceed {SharedKernelFieldLengths.UniversityCode} characters.", nameof(code));

    if (normalizedName.Length > SharedKernelFieldLengths.UniversityName)
      throw new ArgumentException($"University name cannot exceed {SharedKernelFieldLengths.UniversityName} characters.", nameof(name));

    return new UniversityRecord
    {
      Code = normalizedCode.ToUpperInvariant(),
      Name = normalizedName,
      IsActive = true
    };
  }

  public void Deactivate() => IsActive = false;

  public void Reactivate() => IsActive = true;
}
