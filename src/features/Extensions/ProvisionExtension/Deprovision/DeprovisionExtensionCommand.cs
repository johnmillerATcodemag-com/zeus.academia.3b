using MediatR;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed record DeprovisionExtensionCommand(int Number) : IRequest<DeprovisionExtensionResponse>;
