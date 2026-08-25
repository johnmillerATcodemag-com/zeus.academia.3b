using MediatR;
using Microsoft.EntityFrameworkCore;
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
     if (request.Number <= 0)
     {
       throw new ArgumentOutOfRangeException(nameof(request.Number), "Extension number must be a positive whole number.");
     }

     var extension = await _dbContext.Extensions
       .SingleOrDefaultAsync(x => x.Number == request.Number, cancellationToken);

     if (extension is null)
     {
       throw new KeyNotFoundException($"Extension number '{request.Number}' was not found.");
     }

     if (extension.AssignedEmpNr is not null)
     {
       throw new ConflictException($"Extension number '{request.Number}' is assigned and cannot be deprovisioned.");
     }

     _dbContext.Extensions.Remove(extension);
     await _dbContext.SaveChangesAsync(cancellationToken);

     return new DeprovisionExtensionResponse(extension.Number, true);
   }
}
