using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed class ProvisionExtensionHandler : IRequestHandler<ProvisionExtensionCommand, ProvisionExtensionResponse>
{
  private readonly ProvisionExtensionDbContext _dbContext;

  public ProvisionExtensionHandler(ProvisionExtensionDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<ProvisionExtensionResponse> Handle(ProvisionExtensionCommand request, CancellationToken cancellationToken)
  {
    var normalizedNumber = ProvisionExtensionCommand.NormalizeNumber(request.ExtNr);
    var extension = Extension.Create(normalizedNumber);

    var duplicateExists = await _dbContext.Extensions
      .AsNoTracking()
      .AnyAsync(x => x.Number == normalizedNumber, cancellationToken);

    if (duplicateExists)
    {
      throw new ExtensionConflictException($"Extension number '{normalizedNumber}' already exists.");
    }

    _dbContext.Extensions.Add(extension);

    try
    {
      await _dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException)
    {
      var duplicateNowExists = await _dbContext.Extensions
        .AsNoTracking()
        .AnyAsync(x => x.Number == normalizedNumber, cancellationToken);

      if (duplicateNowExists)
      {
        throw new ExtensionConflictException($"Extension number '{normalizedNumber}' already exists.");
      }

      throw;
    }

    return extension.ToResponse();
  }
}
