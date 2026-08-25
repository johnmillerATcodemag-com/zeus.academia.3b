using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public static class DeprovisionExtensionMappings
{
    public static int ToCanonicalNumber(this DeprovisionExtensionCommand command)
    {
        return ExtensionNumberNormalizer.Normalize(command.Number);
    }

    public static DeprovisionExtensionResponse ToResponse(this Extension extension, bool wasRemoved)
    {
        return new DeprovisionExtensionResponse(extension.Number, wasRemoved);
    }
}
