using MediatR;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed record ProvisionExtensionCommand(decimal ExtNr) : IRequest<ProvisionExtensionResponse>;
