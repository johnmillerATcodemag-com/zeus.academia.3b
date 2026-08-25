using FluentValidation;
using Zeus.Academia.Features.Extensions.ProvisionExtension;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed class DeprovisionExtensionCommandValidator : AbstractValidator<DeprovisionExtensionCommand>
{
  public DeprovisionExtensionCommandValidator()
  {
    RuleFor(x => x.Number)
      .Custom((value, context) =>
      {
        if (ExtensionNumberNormalizer.TryNormalize(value, out _))
        {
          return;
        }

        context.AddFailure(nameof(DeprovisionExtensionCommand.Number), "Extension number must be a positive whole number between 1 and 2147483647.");
      });
  }
}
