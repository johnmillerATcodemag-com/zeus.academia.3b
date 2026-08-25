using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public static class ProvisionExtensionMappings
{
   public static int NormalizeNumber(this decimal extNr)
   {
      if (extNr <= 0)
      {
         throw new ArgumentOutOfRangeException(nameof(extNr), extNr, "Extension number must be greater than zero.");
      }

      if (extNr != decimal.Truncate(extNr))
      {
         throw new ArgumentException("Extension number must be a whole number.", nameof(extNr));
      }

      if (extNr > int.MaxValue)
      {
         throw new ArgumentOutOfRangeException(nameof(extNr), extNr, $"Extension number must be between 1 and {int.MaxValue}.");
      }

      return (int)extNr;
   }

   public static Extension ToExtension(this ProvisionExtensionCommand command)
   {
      var number = command.ExtNr.NormalizeNumber();
      return Extension.Create(number);
   }

   public static ProvisionExtensionResponse ToResponse(this Extension extension)
   {
      return new ProvisionExtensionResponse(extension.Number);
   }
}
