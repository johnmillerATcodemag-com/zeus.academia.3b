namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public static class DeprovisionExtensionMappings
{
   public static DeprovisionExtensionResponse ToResponse(this Zeus.Academia.Features.SharedKernel.Foundation.Domain.Extension extension)
   {
     return new DeprovisionExtensionResponse(extension.Number, true);
   }
}
