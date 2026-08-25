using MediatR;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed record DeprovisionExtensionCommand(decimal Number) : IRequest<DeprovisionExtensionResponse>;
