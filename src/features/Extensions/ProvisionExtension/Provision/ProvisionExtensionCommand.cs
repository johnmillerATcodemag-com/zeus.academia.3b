using MediatR;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed record ProvisionExtensionCommand(decimal ExtNr) : IRequest<ProvisionExtensionResponse>;
