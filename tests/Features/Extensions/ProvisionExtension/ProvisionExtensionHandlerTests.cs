using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionHandlerTests
{
   [Fact]
   public async Task Handle_WhenNumberIsUnique_PersistsExtension()
   {
     await using var dbContext = CreateInMemoryContext();
     var handler = new ProvisionExtensionHandler(dbContext);

     var response = await handler.Handle(new ProvisionExtensionCommand(1001m), CancellationToken.None);

     Assert.Equal(1001, response.Number);
     Assert.Null(response.AssignedEmpNr);

     var persisted = await dbContext.Extensions.SingleAsync(x => x.Number == 1001);
     Assert.Equal(1001, persisted.Number);
   }

   [Fact]
   public async Task Handle_WhenNumberAlreadyExists_ThrowsConflictExceptionWithoutDuplicate()
   {
     await using var dbContext = CreateInMemoryContext();
     dbContext.Extensions.Add(Extension.Create(1001));
     await dbContext.SaveChangesAsync();

     var handler = new ProvisionExtensionHandler(dbContext);

     var exception = await Assert.ThrowsAsync<ConflictException>(async () =>
       await handler.Handle(new ProvisionExtensionCommand(1001m), CancellationToken.None));

     Assert.Contains("already provisioned", exception.Message, StringComparison.OrdinalIgnoreCase);
     Assert.Equal(1, await dbContext.Extensions.CountAsync());
   }

   private static ProvisionExtensionDbContext CreateInMemoryContext()
   {
     var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
       .UseInMemoryDatabase($"ProvisionExtensionHandlerTests-{Guid.NewGuid():N}")
       .Options;

     return new ProvisionExtensionDbContext(options);
   }
}
