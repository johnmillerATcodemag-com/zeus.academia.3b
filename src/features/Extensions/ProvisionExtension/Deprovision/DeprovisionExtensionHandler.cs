using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed class DeprovisionExtensionHandler(ProvisionExtensionDbContext dbContext)
   : IRequestHandler<DeprovisionExtensionCommand, DeprovisionExtensionResponse>
{
   public async Task<DeprovisionExtensionResponse> Handle(
      DeprovisionExtensionCommand request,
      CancellationToken cancellationToken)
   {
      var number = request.Number;

      if (number <= 0)
      {
         throw new ArgumentOutOfRangeException(nameof(request.Number), number, "Extension number must be greater than zero.");
      }

      if (number != decimal.Truncate(number))
      {
         throw new ArgumentException("Extension number must be a whole number.", nameof(request.Number));
      }

      if (number > int.MaxValue)
      {
         throw new ArgumentOutOfRangeException(nameof(request.Number), number, $"Extension number must be between 1 and {int.MaxValue}.");
      }

      var extension = await dbContext.Extensions
         .SingleOrDefaultAsync(x => x.Number == (int)number, cancellationToken);

      if (extension is null)
      {
         throw new KeyNotFoundException($"Extension {number} was not found.");
      }

      if (extension.AssignedEmpNr is not null)
      {
         throw new ConflictException($"Extension {extension.Number} is assigned to '{extension.AssignedEmpNr}' and cannot be deprovisioned.");
      }

      dbContext.Extensions.Remove(extension);
      await dbContext.SaveChangesAsync(cancellationToken);

      return new DeprovisionExtensionResponse(extension.Number, true);
   }
}
