using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionDbContextModelTests
{
  [Fact]
  public void Extensions_HasPrimaryKeyOnNumber_AndNoDuplicatePrimaryKeyIndex()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType(typeof(Extension));

    Assert.NotNull(entityType);

    var primaryKey = entityType!.FindPrimaryKey();
    Assert.NotNull(primaryKey);
    Assert.Single(primaryKey.Properties);
    Assert.Equal("Number", primaryKey.Properties[0].Name);
    Assert.Equal(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never, primaryKey.Properties[0].ValueGenerated);

    var duplicatePkUniqueIndex = entityType.GetIndexes().Any(index =>
      index.IsUnique &&
      index.Properties.Count == primaryKey.Properties.Count &&
      index.Properties.Select(p => p.Name).SequenceEqual(primaryKey.Properties.Select(p => p.Name)));

    Assert.False(duplicatePkUniqueIndex);
  }

  [Fact]
  public void Extensions_HasFilteredUniqueIndexOnAssignedEmpNr()
  {
    using var context = CreateContext();
    var entityType = context.Model.FindEntityType(typeof(Extension));

    Assert.NotNull(entityType);

    var index = entityType!.GetIndexes()
      .SingleOrDefault(x => x.Properties.Select(p => p.Name).SequenceEqual(["AssignedEmpNr"]));

    Assert.NotNull(index);
    Assert.True(index!.IsUnique);
    Assert.Equal("[AssignedEmpNr] IS NOT NULL", index.GetFilter());
  }

  private static ProvisionExtensionDbContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
      .UseInMemoryDatabase($"ProvisionExtensionModelTests-{Guid.NewGuid():N}")
      .Options;

    return new ProvisionExtensionDbContext(options);
  }
}
