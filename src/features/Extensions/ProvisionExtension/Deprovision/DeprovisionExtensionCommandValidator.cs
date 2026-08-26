using FluentValidation;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed class DeprovisionExtensionCommandValidator : AbstractValidator<DeprovisionExtensionCommand>
{
  public DeprovisionExtensionCommandValidator()
  {
    RuleFor(x => x.Number)
      .Cascade(CascadeMode.Stop)
      .GreaterThan(0)
      .WithMessage("Extension number must be greater than zero.");
  }
}
