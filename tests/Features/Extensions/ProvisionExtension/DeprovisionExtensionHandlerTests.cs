using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class DeprovisionExtensionHandlerTests
{
  [Fact]
  public async Task Handle_WhenExtensionIsUnassigned_RemovesIt()
  {
    await using var dbContext = CreateInMemoryContext();
    dbContext.Extensions.Add(Extension.Create(42));
    await dbContext.SaveChangesAsync();

    var handler = new DeprovisionExtensionHandler(dbContext);

    var response = await handler.Handle(new DeprovisionExtensionCommand(42), CancellationToken.None);

    Assert.True(response.Removed);
    Assert.Equal(42, response.Number);
    Assert.Empty(await dbContext.Extensions.AsNoTracking().ToListAsync());
  }

  [Fact]
  public async Task Handle_WhenExtensionDoesNotExist_ThrowsNotFoundException()
  {
    await using var dbContext = CreateInMemoryContext();
    var handler = new DeprovisionExtensionHandler(dbContext);

    var exception = await Assert.ThrowsAsync<ExtensionNotFoundException>(async () =>
      await handler.Handle(new DeprovisionExtensionCommand(42), CancellationToken.None));

    Assert.Contains("does not exist", exception.Message, StringComparison.OrdinalIgnoreCase);
    Assert.Equal(0, await dbContext.Extensions.CountAsync());
  }

  [Fact]
  public async Task Handle_WhenExtensionIsAssigned_ThrowsConflictAndPreservesAssignment()
  {
    await using var dbContext = CreateInMemoryContext();
    var extension = Extension.Create(42);
    extension.AssignTo("123");
    dbContext.Extensions.Add(extension);
    await dbContext.SaveChangesAsync();

    var handler = new DeprovisionExtensionHandler(dbContext);

    var exception = await Assert.ThrowsAsync<ConflictException>(async () =>
      await handler.Handle(new DeprovisionExtensionCommand(42), CancellationToken.None));

    Assert.Contains("assigned", exception.Message, StringComparison.OrdinalIgnoreCase);

    var persisted = await dbContext.Extensions.SingleAsync();
    Assert.Equal(42, persisted.Number);
    Assert.Equal("123", persisted.AssignedEmpNr);
  }

  private static ProvisionExtensionDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
      .UseInMemoryDatabase($"DeprovisionExtensionTests-{Guid.NewGuid():N}")
      .Options;

    return new ProvisionExtensionDbContext(options);
  }
}
