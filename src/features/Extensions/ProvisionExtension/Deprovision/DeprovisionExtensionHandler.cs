using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;
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
    var extension = await _dbContext.Extensions
      .SingleOrDefaultAsync(x => x.Number == request.Number, cancellationToken);

    if (extension is null)
    {
      throw new ExtensionNotFoundException($"Extension number '{request.Number}' does not exist.");
    }

    if (!extension.IsAvailable)
    {
      throw new ConflictException($"Extension number '{request.Number}' is assigned to academic '{extension.AssignedEmpNr}' and cannot be deprovisioned.");
    }

    _dbContext.Extensions.Remove(extension);
    await _dbContext.SaveChangesAsync(cancellationToken);

    return new DeprovisionExtensionResponse(extension.Number, true);
  }
}
