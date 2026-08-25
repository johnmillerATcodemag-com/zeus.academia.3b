using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public static class ProvisionExtensionMappings
{
   public static Extension ToExtension(this ProvisionExtensionCommand command)
   {
     var number = ExtensionNumberNormalizer.Normalize(command.Number);
     return Extension.Create(number);
   }

   public static ProvisionExtensionResponse ToResponse(this Extension extension)
   {
     return new ProvisionExtensionResponse(extension.Number, extension.AssignedEmpNr);
   }
}
