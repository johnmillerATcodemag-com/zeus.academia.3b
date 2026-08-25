using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed class DeprovisionExtensionHandler : IRequestHandler<DeprovisionExtensionCommand, DeprovisionExtensionResponse>
{
  private readonly ProvisionExtensionDbContext _dbContext;

  public DeprovisionExtensionHandler(ProvisionExtensionDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<DeprovisionExtensionResponse> Handle(DeprovisionExtensionCommand request, CancellationToken cancellationToken)
  {
    var normalizedNumber = ExtensionNumberNormalizer.Normalize(request.Number, nameof(request.Number));

    var extension = await _dbContext.Extensions
      .SingleOrDefaultAsync(x => x.Number == normalizedNumber, cancellationToken);

    if (extension is null)
    {
      throw new KeyNotFoundException($"Extension {normalizedNumber} was not found.");
    }

    if (extension.AssignedEmpNr is not null)
    {
      throw new ConflictException($"Extension {normalizedNumber} is assigned to '{extension.AssignedEmpNr}' and cannot be deprovisioned.");
    }

    _dbContext.Extensions.Remove(extension);
    await _dbContext.SaveChangesAsync(cancellationToken);

    return extension.ToResponse();
  }
}
