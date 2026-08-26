using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

public sealed class ManageUniversitiesDbContextModelTests
{
  [Fact]
  public void Universities_HasPrimaryKeyOnCode_AndNoDuplicateUniqueIndex()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType(typeof(UniversityRecord));

    Assert.NotNull(entityType);

    var primaryKey = entityType!.FindPrimaryKey();
    Assert.NotNull(primaryKey);
    Assert.Single(primaryKey!.Properties);
    Assert.Equal("Code", primaryKey.Properties[0].Name);

    var duplicatePkUniqueIndex = entityType.GetIndexes().Any(index =>
      index.IsUnique &&
      index.Properties.Count == primaryKey.Properties.Count &&
      index.Properties.Select(p => p.Name).SequenceEqual(primaryKey.Properties.Select(p => p.Name)));

    Assert.False(duplicatePkUniqueIndex);
  }

  [Fact]
  public void Universities_CodeAndName_AreRequiredWithCanonicalLengths()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType(typeof(UniversityRecord));
    Assert.NotNull(entityType);

    var codeProperty = entityType!.FindProperty(nameof(UniversityRecord.Code));
    var nameProperty = entityType.FindProperty(nameof(UniversityRecord.Name));

    Assert.NotNull(codeProperty);
    Assert.NotNull(nameProperty);
    Assert.False(codeProperty!.IsNullable);
    Assert.False(nameProperty!.IsNullable);
    Assert.Equal(SharedKernelFieldLengths.UniversityCode, codeProperty.GetMaxLength());
    Assert.Equal(SharedKernelFieldLengths.UniversityName, nameProperty.GetMaxLength());
  }

  [Fact]
  public void Universities_HasAllowedCodeCheckConstraint_DerivedFromCanonicalCatalog()
  {
    using var context = CreateContext();
    var createScript = context.Database.GenerateCreateScript();

    Assert.Contains("CK_Universities_Code_Allowed", createScript, StringComparison.Ordinal);

    foreach (var university in UniversityCodeCatalog.SupportedUniversities)
    {
      Assert.Contains($"'{university.Code}'", createScript, StringComparison.Ordinal);
    }
  }

  [Fact]
  public void SupportedUniversities_AreExposedAsImmutableReadOnlyCollection()
  {
    var universities = UniversityCodeCatalog.SupportedUniversities;

    Assert.IsAssignableFrom<IReadOnlyList<UniversityCatalogEntry>>(universities);
    Assert.False(universities is UniversityCatalogEntry[]);
  }

  private static ManageUniversitiesDbContext CreateContext()
  {
    var connectionString = Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
      if (OperatingSystem.IsWindows())
      {
        connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ZeusAcademiaManageUniversitiesDesign;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
      }
      else
      {
        throw new InvalidOperationException("ZEUS_SQLSERVER_CONNECTION is required on non-Windows hosts because SQL Server LocalDB is unavailable.");
      }
    }

    var options = new DbContextOptionsBuilder<ManageUniversitiesDbContext>()
      .UseSqlServer(connectionString)
      .Options;

    return new ManageUniversitiesDbContext(options);
  }
}