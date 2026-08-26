using MediatR;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed record ProvisionExtensionCommand(decimal ExtNr) : IRequest<ProvisionExtensionResponse>
{
  public static int NormalizeNumber(decimal extNr)
  {
    if (extNr <= 0m)
    {
      throw new ArgumentOutOfRangeException(nameof(extNr), "Extension number must be greater than zero.");
    }

    if (extNr != decimal.Truncate(extNr))
    {
      throw new ArgumentException("Extension number must be a whole number. Fractional values are not allowed.", nameof(extNr));
    }

    if (extNr > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(extNr), $"Extension number must be between 1 and {int.MaxValue}.");
    }

    return (int)extNr;
  }
}
