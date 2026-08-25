using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class DeprovisionExtensionHandlerTests
{
  [Fact]
  public async Task Handle_WithUnassignedExtension_RemovesExtensionFromPool()
  {
    await using var dbContext = CreateInMemoryContext();
    dbContext.Extensions.Add(Extension.Create(101));
    await dbContext.SaveChangesAsync();

    var sut = new DeprovisionExtensionHandler(dbContext);

    var response = await sut.Handle(new DeprovisionExtensionCommand(101m), CancellationToken.None);

    Assert.Equal(101, response.Number);
    Assert.Empty(await dbContext.Extensions.ToListAsync());
  }

  [Fact]
  public async Task Handle_WithMissingExtension_ThrowsKeyNotFoundException()
  {
    await using var dbContext = CreateInMemoryContext();
    var sut = new DeprovisionExtensionHandler(dbContext);

    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
      sut.Handle(new DeprovisionExtensionCommand(101m), CancellationToken.None));

    Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task Handle_WithAssignedExtension_ThrowsConflictExceptionAndPreservesAssignment()
  {
    await using var dbContext = CreateInMemoryContext();
    var extension = Extension.Create(101);
    extension.AssignTo("EMP001");
    dbContext.Extensions.Add(extension);
    await dbContext.SaveChangesAsync();

    var sut = new DeprovisionExtensionHandler(dbContext);

    var exception = await Assert.ThrowsAsync<ConflictException>(() =>
      sut.Handle(new DeprovisionExtensionCommand(101m), CancellationToken.None));

    Assert.Contains("assigned", exception.Message, StringComparison.OrdinalIgnoreCase);

    var persisted = await dbContext.Extensions.SingleAsync(x => x.Number == 101);
    Assert.Equal("EMP001", persisted.AssignedEmpNr);
  }

  private static ProvisionExtensionDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
      .UseInMemoryDatabase($"ProvisionExtensionTests-{Guid.NewGuid():N}")
      .Options;

    return new ProvisionExtensionDbContext(options);
  }
}