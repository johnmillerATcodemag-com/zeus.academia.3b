using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionDbContextModelTests
{
   [Fact]
   public void Model_UsesSharedKernelExtensionConfigurationForExtensionsTable()
   {
     var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
       .UseInMemoryDatabase($"ProvisionExtensionModelTests-{Guid.NewGuid():N}")
       .Options;

     using var dbContext = new ProvisionExtensionDbContext(options);
     var model = dbContext.Model;
     var entity = model.FindEntityType(typeof(Extension));

     Assert.NotNull(entity);
     Assert.Equal("Extensions", entity.GetTableName());

     var primaryKey = entity.FindPrimaryKey();
     Assert.NotNull(primaryKey);
     Assert.Equal(nameof(Extension.Number), primaryKey.Properties.Single().Name);

     var assignmentIndex = entity.GetIndexes()
       .Single(x => x.Properties.Single().Name == nameof(Extension.AssignedEmpNr));

     Assert.True(assignmentIndex.IsUnique);
     Assert.Equal("[AssignedEmpNr] IS NOT NULL", assignmentIndex.GetFilter());
   }
}
