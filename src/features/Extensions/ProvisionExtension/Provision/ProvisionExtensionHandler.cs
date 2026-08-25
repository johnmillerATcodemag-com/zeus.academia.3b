using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed class ProvisionExtensionHandler(ProvisionExtensionDbContext dbContext)
   : IRequestHandler<ProvisionExtensionCommand, ProvisionExtensionResponse>
{
   public async Task<ProvisionExtensionResponse> Handle(
      ProvisionExtensionCommand request,
      CancellationToken cancellationToken)
   {
      var number = request.ExtNr.NormalizeNumber();

      var alreadyExists = await dbContext.Extensions
         .AsNoTracking()
         .AnyAsync(x => x.Number == number, cancellationToken);

      if (alreadyExists)
      {
         throw new ConflictException($"Extension {number} is already provisioned.");
      }

      var extension = Extension.Create(number);
      dbContext.Extensions.Add(extension);

      try
      {
         await dbContext.SaveChangesAsync(cancellationToken);
      }
      catch (DbUpdateException)
      {
         var existsNow = await dbContext.Extensions
            .AsNoTracking()
            .AnyAsync(x => x.Number == number, cancellationToken);

         if (existsNow)
         {
            throw new ConflictException($"Extension {number} is already provisioned.");
         }

         throw;
      }

      return extension.ToResponse();
   }
}
