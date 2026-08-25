using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class DeprovisionExtensionHandlerTests
{
   [Fact]
   public async Task Handle_WhenExtensionExistsAndIsAvailable_RemovesIt()
   {
      await using var dbContext = CreateInMemoryContext();
      dbContext.Extensions.Add(Extension.Create(42));
      await dbContext.SaveChangesAsync();

      var handler = new DeprovisionExtensionHandler(dbContext);

      var response = await handler.Handle(new DeprovisionExtensionCommand(42m), CancellationToken.None);

      Assert.Equal(42, response.Number);
      Assert.True(response.Removed);
      Assert.Empty(await dbContext.Extensions.ToListAsync());
   }

   [Fact]
   public async Task Handle_WhenExtensionDoesNotExist_ThrowsKeyNotFoundException()
   {
      await using var dbContext = CreateInMemoryContext();
      var handler = new DeprovisionExtensionHandler(dbContext);

      await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
         await handler.Handle(new DeprovisionExtensionCommand(42m), CancellationToken.None));
   }

   [Fact]
   public async Task Handle_WhenExtensionIsAssigned_ThrowsConflictAndPreservesAssignment()
   {
      await using var dbContext = CreateInMemoryContext();
      var extension = Extension.Create(42);
      extension.AssignTo("A00001");
      dbContext.Extensions.Add(extension);
      await dbContext.SaveChangesAsync();

      var handler = new DeprovisionExtensionHandler(dbContext);

      var exception = await Assert.ThrowsAsync<ConflictException>(async () =>
         await handler.Handle(new DeprovisionExtensionCommand(42m), CancellationToken.None));

      Assert.Contains("assigned", exception.Message, StringComparison.OrdinalIgnoreCase);

      var persisted = await dbContext.Extensions.SingleAsync(x => x.Number == 42);
      Assert.Equal("A00001", persisted.AssignedEmpNr);
   }

   private static ProvisionExtensionDbContext CreateInMemoryContext()
   {
      var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
         .UseInMemoryDatabase($"DeprovisionExtensionTests-{Guid.NewGuid():N}")
         .Options;

      return new ProvisionExtensionDbContext(options);
   }
}
