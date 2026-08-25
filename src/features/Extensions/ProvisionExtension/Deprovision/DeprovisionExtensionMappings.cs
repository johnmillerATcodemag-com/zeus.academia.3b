using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public static class DeprovisionExtensionMappings
{
  public static DeprovisionExtensionResponse ToResponse(this Extension extension)
  {
    return new DeprovisionExtensionResponse(extension.Number);
  }
}
