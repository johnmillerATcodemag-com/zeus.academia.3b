using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed class ProvisionExtensionHandler(ProvisionExtensionDbContext dbContext)
    : IRequestHandler<ProvisionExtensionCommand, ProvisionExtensionResponse>
{
    public async Task<ProvisionExtensionResponse> Handle(ProvisionExtensionCommand request, CancellationToken cancellationToken)
    {
        var extension = request.ToExtension();

        var alreadyExists = await dbContext.Extensions
            .AsNoTracking()
            .AnyAsync(x => x.Number == extension.Number, cancellationToken);

        if (alreadyExists)
        {
            throw new ConflictException($"Extension number {extension.Number} already exists.");
        }

        dbContext.Extensions.Add(extension);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var duplicateExists = await dbContext.Extensions
                .AsNoTracking()
                .AnyAsync(x => x.Number == extension.Number, cancellationToken);

            if (duplicateExists)
            {
                throw new ConflictException($"Extension number {extension.Number} already exists.");
            }

            throw;
        }

        return extension.ToResponse();
    }
}
