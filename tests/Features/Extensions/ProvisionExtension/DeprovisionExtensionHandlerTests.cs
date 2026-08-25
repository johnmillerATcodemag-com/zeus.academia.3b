using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class DeprovisionExtensionHandlerTests
{
   [Fact]
   public async Task Handle_WhenExtensionIsUnassigned_RemovesIt()
   {
     await using var dbContext = CreateInMemoryContext();
     dbContext.Extensions.Add(Extension.Create(1010));
     await dbContext.SaveChangesAsync();

     var handler = new DeprovisionExtensionHandler(dbContext);

     var response = await handler.Handle(new DeprovisionExtensionCommand(1010), CancellationToken.None);

     Assert.Equal(1010, response.Number);
     Assert.True(response.Removed);
     Assert.False(await dbContext.Extensions.AnyAsync(x => x.Number == 1010));
   }

   [Fact]
   public async Task Handle_WhenExtensionDoesNotExist_ThrowsKeyNotFoundException()
   {
     await using var dbContext = CreateInMemoryContext();
     var handler = new DeprovisionExtensionHandler(dbContext);

     await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
       await handler.Handle(new DeprovisionExtensionCommand(4040), CancellationToken.None));
   }

   [Fact]
   public async Task Handle_WhenExtensionIsAssigned_ThrowsConflictExceptionAndPreservesAssignment()
   {
     await using var dbContext = CreateInMemoryContext();
     var extension = Extension.Create(2020);
     extension.AssignTo("EMP001");
     dbContext.Extensions.Add(extension);
     await dbContext.SaveChangesAsync();

     var handler = new DeprovisionExtensionHandler(dbContext);

     var exception = await Assert.ThrowsAsync<ConflictException>(async () =>
       await handler.Handle(new DeprovisionExtensionCommand(2020), CancellationToken.None));

     Assert.Contains("assigned", exception.Message, StringComparison.OrdinalIgnoreCase);
     var persisted = await dbContext.Extensions.SingleAsync(x => x.Number == 2020);
     Assert.Equal("EMP001", persisted.AssignedEmpNr);
   }

   private static ProvisionExtensionDbContext CreateInMemoryContext()
   {
     var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
       .UseInMemoryDatabase($"DeprovisionExtensionHandlerTests-{Guid.NewGuid():N}")
       .Options;

     return new ProvisionExtensionDbContext(options);
   }
}
