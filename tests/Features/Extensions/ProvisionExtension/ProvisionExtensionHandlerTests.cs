using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionHandlerTests
{
   [Fact]
   public async Task Handle_WhenExtensionIsAvailable_PersistsNewExtension()
   {
      await using var dbContext = CreateInMemoryContext();
      var handler = new ProvisionExtensionHandler(dbContext);

      var response = await handler.Handle(new ProvisionExtensionCommand(42m), CancellationToken.None);

      Assert.Equal(42, response.Number);

      var persisted = await dbContext.Extensions.SingleAsync(x => x.Number == 42);
      Assert.Equal(42, persisted.Number);
      Assert.Null(persisted.AssignedEmpNr);
   }

   [Fact]
   public async Task Handle_WhenExtensionAlreadyExists_ThrowsConflict_AndLeavesRecordUnchanged()
   {
      await using var dbContext = CreateInMemoryContext();
      dbContext.Extensions.Add(Extension.Create(42));
      await dbContext.SaveChangesAsync();

      var handler = new ProvisionExtensionHandler(dbContext);

      var exception = await Assert.ThrowsAsync<ConflictException>(async () =>
         await handler.Handle(new ProvisionExtensionCommand(42m), CancellationToken.None));

      Assert.Contains("already provisioned", exception.Message, StringComparison.OrdinalIgnoreCase);
      Assert.Equal(1, await dbContext.Extensions.CountAsync());
   }

   private static ProvisionExtensionDbContext CreateInMemoryContext()
   {
      var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
         .UseInMemoryDatabase($"ProvisionExtensionTests-{Guid.NewGuid():N}")
         .Options;

      return new ProvisionExtensionDbContext(options);
   }
}
