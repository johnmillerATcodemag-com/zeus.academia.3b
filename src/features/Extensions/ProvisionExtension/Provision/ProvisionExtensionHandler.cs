using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

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
    var extension = request.ToExtension();

    var alreadyExists = await _dbContext.Extensions
      .AsNoTracking()
      .AnyAsync(x => x.Number == extension.Number, cancellationToken);

    if (alreadyExists)
    {
      throw new ConflictException($"Extension {extension.Number} is already provisioned.");
    }

    _dbContext.Extensions.Add(extension);

    try
    {
      await _dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException)
    {
      var existsNow = await _dbContext.Extensions
        .AsNoTracking()
        .AnyAsync(x => x.Number == extension.Number, cancellationToken);

      if (existsNow)
      {
        throw new ConflictException($"Extension {extension.Number} is already provisioned.");
      }

      throw;
    }

    return extension.ToResponse();
  }
}
