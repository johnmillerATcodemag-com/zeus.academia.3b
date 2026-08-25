using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionDbContextModelTests
{
  [Fact]
  public void Extension_Model_UsesNumberPrimaryKey_AndFilteredAssignmentIndex()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType(typeof(Extension));

    Assert.NotNull(entityType);
    Assert.Equal("Extensions", entityType!.GetTableName());

    var primaryKey = entityType.FindPrimaryKey();
    Assert.NotNull(primaryKey);
    Assert.Single(primaryKey!.Properties);
    Assert.Equal(nameof(Extension.Number), primaryKey.Properties[0].Name);

    var assignedIndex = Assert.Single(entityType.GetIndexes().Where(index =>
      index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(Extension.AssignedEmpNr) })));

    Assert.True(assignedIndex.IsUnique);
    Assert.Equal("[AssignedEmpNr] IS NOT NULL", assignedIndex.GetFilter());

    var createScript = context.Database.GenerateCreateScript();
    Assert.Contains("CREATE TABLE [Extensions]", createScript, StringComparison.Ordinal);
    Assert.Contains("PRIMARY KEY ([Number])", createScript, StringComparison.Ordinal);
    Assert.Contains("WHERE [AssignedEmpNr] IS NOT NULL", createScript, StringComparison.Ordinal);
  }

  [Fact]
  public void Extension_Model_DoesNotIntroduceDuplicateUniqueKeyIndex()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType(typeof(Extension));

    Assert.NotNull(entityType);

    Assert.DoesNotContain(entityType!.GetIndexes(), index =>
      index.IsUnique &&
      index.Properties.Count == 1 &&
      index.Properties[0].Name == nameof(Extension.Number));
  }

  private static ProvisionExtensionDbContext CreateContext()
  {
    var connectionString = Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
      if (OperatingSystem.IsWindows())
      {
        connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ZeusAcademiaProvisionExtensionDesign;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
      }
      else
      {
        throw new InvalidOperationException("ZEUS_SQLSERVER_CONNECTION is required on non-Windows hosts because SQL Server LocalDB is unavailable.");
      }
    }

    var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
      .UseSqlServer(connectionString)
      .Options;

    return new ProvisionExtensionDbContext(options);
  }
}