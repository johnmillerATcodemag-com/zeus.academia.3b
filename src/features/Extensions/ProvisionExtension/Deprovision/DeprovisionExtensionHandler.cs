using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed class DeprovisionExtensionHandler(ProvisionExtensionDbContext dbContext)
    : IRequestHandler<DeprovisionExtensionCommand, DeprovisionExtensionResponse>
{
    public async Task<DeprovisionExtensionResponse> Handle(DeprovisionExtensionCommand request, CancellationToken cancellationToken)
    {
        var normalizedNumber = ExtensionNumberNormalizer.Normalize(request.Number);

        var extension = await dbContext.Extensions
            .SingleOrDefaultAsync(x => x.Number == normalizedNumber, cancellationToken);

        if (extension is null)
        {
            return new DeprovisionExtensionResponse(normalizedNumber, false);
        }

        if (extension.AssignedEmpNr is not null)
        {
            throw new ConflictException($"Extension number {extension.Number} is assigned to '{extension.AssignedEmpNr}' and cannot be deprovisioned.");
        }

        dbContext.Extensions.Remove(extension);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeprovisionExtensionResponse(extension.Number, true);
    }
}
