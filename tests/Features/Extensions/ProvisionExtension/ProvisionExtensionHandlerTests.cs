using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionHandlerTests
{
    [Fact]
    public async Task Handle_WhenNumberIsValid_PersistsExtension()
    {
        await using var dbContext = CreateInMemoryContext();
        var handler = new ProvisionExtensionHandler(dbContext);

        var response = await handler.Handle(new ProvisionExtensionCommand(123m), CancellationToken.None);

        Assert.Equal(123, response.Number);
        var persisted = await dbContext.Extensions.SingleAsync(x => x.Number == 123);
        Assert.Equal(123, persisted.Number);
    }

    [Fact]
    public async Task Handle_WhenDuplicateNumberExists_ThrowsConflictException()
    {
        await using var dbContext = CreateInMemoryContext();
        var extension = Zeus.Academia.Features.SharedKernel.Foundation.Domain.Extension.Create(123);
        dbContext.Extensions.Add(extension);
        await dbContext.SaveChangesAsync();

        var handler = new ProvisionExtensionHandler(dbContext);

        var exception = await Assert.ThrowsAsync<ConflictException>(async () =>
            await handler.Handle(new ProvisionExtensionCommand(123m), CancellationToken.None));

        Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ProvisionExtensionDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
            .UseInMemoryDatabase($"ProvisionExtensionTests-{Guid.NewGuid():N}")
            .Options;

        return new ProvisionExtensionDbContext(options);
    }
}
