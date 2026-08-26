using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionSqlServerIntegrationTests
{
  [Fact]
  public async Task ProvisionExtension_PersistsWholeNumberToSqlServer()
  {
    await using var database = await ProvisionExtensionSqlServerTestDatabase.CreateAsync();
    await using var writeContext = database.CreateContext();
    var handler = new ProvisionExtensionHandler(writeContext);

    var response = await handler.Handle(new ProvisionExtensionCommand(42m), CancellationToken.None);

    await using var readContext = database.CreateContext();
    var persisted = await readContext.Extensions.SingleAsync(x => x.Number == 42);

    Assert.Equal(42, response.Number);
    Assert.True(response.IsAvailable);
    Assert.Equal(42, persisted.Number);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  [InlineData(1.5)]
  [InlineData(2147483648)]
  public async Task ProvisionExtension_WhenNumberIsInvalid_RejectsBeforePersistence(decimal number)
  {
    await using var database = await ProvisionExtensionSqlServerTestDatabase.CreateAsync();
    await using var context = database.CreateContext();
    var handler = new ProvisionExtensionHandler(context);

    await Assert.ThrowsAnyAsync<ArgumentException>(() =>
      handler.Handle(new ProvisionExtensionCommand(number), CancellationToken.None));

    Assert.Equal(0, await context.Extensions.CountAsync());
  }

  [Fact]
  public async Task ProvisionExtension_WhenDuplicateNumberExists_RejectsWithoutSecondRecord()
  {
    await using var database = await ProvisionExtensionSqlServerTestDatabase.CreateAsync();
    await using var seedContext = database.CreateContext();
    seedContext.Extensions.Add(Extension.Create(42));
    await seedContext.SaveChangesAsync();

    await using var writeContext = database.CreateContext();
    var handler = new ProvisionExtensionHandler(writeContext);

    await Assert.ThrowsAsync<ExtensionConflictException>(() =>
      handler.Handle(new ProvisionExtensionCommand(42m), CancellationToken.None));

    await using var readContext = database.CreateContext();
    Assert.Equal(1, await readContext.Extensions.CountAsync(x => x.Number == 42));
  }

  [Fact]
  public async Task DeprovisionExtension_WhenUnassigned_RemovesRecordFromSqlServer()
  {
    await using var database = await ProvisionExtensionSqlServerTestDatabase.CreateAsync();
    await using var seedContext = database.CreateContext();
    seedContext.Extensions.Add(Extension.Create(42));
    await seedContext.SaveChangesAsync();

    await using var writeContext = database.CreateContext();
    var handler = new DeprovisionExtensionHandler(writeContext);

    var response = await handler.Handle(new DeprovisionExtensionCommand(42), CancellationToken.None);

    await using var readContext = database.CreateContext();
    Assert.True(response.Removed);
    Assert.False(await readContext.Extensions.AnyAsync(x => x.Number == 42));
  }

  [Fact]
  public async Task DeprovisionExtension_WhenAssigned_RejectsAndPreservesAssignment()
  {
    await using var database = await ProvisionExtensionSqlServerTestDatabase.CreateAsync();
    await using var seedContext = database.CreateContext();
    var extension = Extension.Create(42);
    extension.AssignTo("123");
    seedContext.Extensions.Add(extension);
    await seedContext.SaveChangesAsync();

    await using var writeContext = database.CreateContext();
    var handler = new DeprovisionExtensionHandler(writeContext);

    await Assert.ThrowsAsync<ConflictException>(() =>
      handler.Handle(new DeprovisionExtensionCommand(42), CancellationToken.None));

    await using var readContext = database.CreateContext();
    var persisted = await readContext.Extensions.SingleAsync(x => x.Number == 42);
    Assert.Equal("123", persisted.AssignedEmpNr);
  }
}
