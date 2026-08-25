using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionHandlerTests
{
  [Fact]
  public async Task Handle_WithNewNumber_PersistsExtension()
  {
    await using var dbContext = CreateInMemoryContext();
    var sut = new ProvisionExtensionHandler(dbContext);

    var response = await sut.Handle(new ProvisionExtensionCommand(101m), CancellationToken.None);

    Assert.Equal(101, response.Number);

    var persisted = await dbContext.Extensions.SingleAsync(x => x.Number == 101);
    Assert.Equal(101, persisted.Number);
    Assert.Null(persisted.AssignedEmpNr);
  }

  [Fact]
  public async Task Handle_WithDuplicateNumber_ThrowsConflictExceptionAndKeepsExistingRow()
  {
    await using var dbContext = CreateInMemoryContext();
    dbContext.Extensions.Add(Extension.Create(101));
    await dbContext.SaveChangesAsync();

    var sut = new ProvisionExtensionHandler(dbContext);

    var exception = await Assert.ThrowsAsync<ConflictException>(() =>
      sut.Handle(new ProvisionExtensionCommand(101m), CancellationToken.None));

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