using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
using Zeus.Academia.Features.Extensions.ProvisionExtension;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public static class ProvisionExtensionMappings
{
  public static Extension ToExtension(this ProvisionExtensionCommand command)
  {
    return Extension.Create(ExtensionNumberNormalizer.Normalize(command.Number, nameof(command.Number)));
  }

  public static ProvisionExtensionResponse ToResponse(this Extension extension)
  {
    return new ProvisionExtensionResponse(extension.Number, extension.AssignedEmpNr);
  }
}
