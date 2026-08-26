using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionHandlerTests
{
  [Fact]
  public async Task Handle_WhenExtensionIsValid_PersistsExtension()
  {
    await using var dbContext = CreateInMemoryContext();
    var handler = new ProvisionExtensionHandler(dbContext);

    var response = await handler.Handle(new ProvisionExtensionCommand(42m), CancellationToken.None);

    Assert.Equal(42, response.Number);
    Assert.True(response.IsAvailable);

    var persistedNumber = await dbContext.Extensions
      .AsNoTracking()
      .Where(x => x.Number == 42)
      .Select(x => x.Number)
      .SingleAsync();

    Assert.Equal(42, persistedNumber);
  }

  [Fact]
  public async Task Handle_WhenDuplicateNumberExists_ThrowsConflictAndDoesNotPersistAnotherRow()
  {
    await using var dbContext = CreateInMemoryContext();
    dbContext.Extensions.Add(Extension.Create(42));
    await dbContext.SaveChangesAsync();

    var handler = new ProvisionExtensionHandler(dbContext);

    var exception = await Assert.ThrowsAsync<ExtensionConflictException>(async () =>
      await handler.Handle(new ProvisionExtensionCommand(42m), CancellationToken.None));

    Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
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
