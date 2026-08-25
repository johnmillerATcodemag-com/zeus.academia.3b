using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Persistence;

namespace Zeus.Academia.Tests.Features.SharedKernel.Foundation;

public sealed class SharedKernelDbContextModelTests
{
  [Fact]
  public void Academic_HasPrimaryKeyOnEmpNr_AndNoDuplicateUniqueIndex()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType("Zeus.Academia.Features.SharedKernel.Foundation.Domain.Academic");

    Assert.NotNull(entityType);

    var primaryKey = entityType!.FindPrimaryKey();
    Assert.NotNull(primaryKey);
    Assert.Single(primaryKey!.Properties);
    Assert.Equal("EmpNr", primaryKey.Properties[0].Name);

    var duplicatePkUniqueIndex = entityType.GetIndexes().Any(index =>
        index.IsUnique &&
        index.Properties.Count == primaryKey.Properties.Count &&
        index.Properties.Select(p => p.Name).SequenceEqual(primaryKey.Properties.Select(p => p.Name)));

    Assert.False(duplicatePkUniqueIndex);
  }

  [Fact]
  public void Academic_HasEmploymentMutualExclusionCheckConstraint()
  {
    using var context = CreateContext();

    var createScript = context.Database.GenerateCreateScript();

    Assert.Contains("CK_Academics_EmploymentMutualExclusion", createScript, StringComparison.Ordinal);
    Assert.Contains("NOT ([IsTenured] = 1 AND [ContractEndDate] IS NOT NULL)", createScript, StringComparison.Ordinal);
  }

  [Fact]
  public void AcademicQualification_HasCompositePrimaryKey()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType("Zeus.Academia.Features.SharedKernel.Foundation.Domain.AcademicQualification");

    Assert.NotNull(entityType);

    var primaryKey = entityType!.FindPrimaryKey();

    Assert.NotNull(primaryKey);
    Assert.Equal(["EmpNr", "DegreeCode"], primaryKey!.Properties.Select(p => p.Name).ToArray());
  }

  private static SharedKernelDbContext CreateContext()
  {
    var connectionString = Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
      if (OperatingSystem.IsWindows())
      {
        connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ZeusAcademiaSharedKernelDesign;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
      }
      else
      {
        throw new InvalidOperationException("ZEUS_SQLSERVER_CONNECTION is required on non-Windows hosts because SQL Server LocalDB is unavailable.");
      }
    }

    var options = new DbContextOptionsBuilder<SharedKernelDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    return new SharedKernelDbContext(options);
  }
}
