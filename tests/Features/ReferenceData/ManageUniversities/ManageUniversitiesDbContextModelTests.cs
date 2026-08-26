using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
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
  public void Universities_HasRequiredAndMaxLengthConfiguration()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType(typeof(UniversityRecord));

    Assert.NotNull(entityType);

    var code = entityType!.FindProperty(nameof(UniversityRecord.Code));
    Assert.NotNull(code);
    Assert.False(code!.IsNullable);
    Assert.Equal(SharedKernelFieldLengths.UniversityCode, code.GetMaxLength());

    var name = entityType.FindProperty(nameof(UniversityRecord.Name));
    Assert.NotNull(name);
    Assert.False(name!.IsNullable);
    Assert.Equal(SharedKernelFieldLengths.UniversityName, name.GetMaxLength());

    var isActive = entityType.FindProperty(nameof(UniversityRecord.IsActive));
    Assert.NotNull(isActive);
    Assert.False(isActive!.IsNullable);
  }

  [Fact]
  public void Universities_HasAllowedCodeCheckConstraint_DerivedFromCanonicalCatalog()
  {
    using var context = CreateContext();

    var createScript = context.Database.GenerateCreateScript();

    Assert.Contains("CK_Universities_Code_Allowed", createScript, StringComparison.Ordinal);

    foreach (var code in UniversityCodeCatalog.SupportedCodes)
    {
      Assert.Contains($"'{code}'", createScript, StringComparison.Ordinal);
    }
  }

  [Fact]
  public void SupportedCodes_AreExposedAsImmutableReadOnlyCollection()
  {
    var codes = UniversityCodeCatalog.SupportedCodes;

    Assert.IsAssignableFrom<IReadOnlyList<string>>(codes);
    Assert.False(codes is string[]);
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