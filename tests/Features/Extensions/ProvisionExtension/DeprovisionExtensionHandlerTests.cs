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
        var extension = Extension.Create(123);
        dbContext.Extensions.Add(extension);
        await dbContext.SaveChangesAsync();

        var handler = new DeprovisionExtensionHandler(dbContext);

        var response = await handler.Handle(new DeprovisionExtensionCommand(123m), CancellationToken.None);

        Assert.True(response.WasRemoved);
        Assert.Equal(123, response.Number);
        Assert.False(await dbContext.Extensions.AnyAsync(x => x.Number == 123));
    }

    [Fact]
    public async Task Handle_WhenExtensionDoesNotExist_ReturnsNotRemovedResponse()
    {
        await using var dbContext = CreateInMemoryContext();
        var handler = new DeprovisionExtensionHandler(dbContext);

        var response = await handler.Handle(new DeprovisionExtensionCommand(123m), CancellationToken.None);

        Assert.False(response.WasRemoved);
        Assert.Equal(123, response.Number);
    }

    [Fact]
    public async Task Handle_WhenExtensionIsAssigned_ThrowsConflictException()
    {
        await using var dbContext = CreateInMemoryContext();
        var extension = Extension.Create(123);
        extension.AssignTo("E123");
        dbContext.Extensions.Add(extension);
        await dbContext.SaveChangesAsync();

        var handler = new DeprovisionExtensionHandler(dbContext);

        var exception = await Assert.ThrowsAsync<ConflictException>(async () =>
            await handler.Handle(new DeprovisionExtensionCommand(123m), CancellationToken.None));

        Assert.Contains("assigned", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("E123", (await dbContext.Extensions.SingleAsync(x => x.Number == 123)).AssignedEmpNr);
    }

    private static ProvisionExtensionDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
            .UseInMemoryDatabase($"DeprovisionExtensionTests-{Guid.NewGuid():N}")
            .Options;

        return new ProvisionExtensionDbContext(options);
    }
}
