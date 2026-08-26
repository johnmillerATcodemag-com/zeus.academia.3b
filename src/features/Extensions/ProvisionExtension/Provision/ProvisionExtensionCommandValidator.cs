using FluentValidation;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed class ProvisionExtensionCommandValidator : AbstractValidator<ProvisionExtensionCommand>
{
  public ProvisionExtensionCommandValidator()
  {
    RuleFor(x => x.ExtNr)
      .Cascade(CascadeMode.Stop)
      .Custom((extNr, context) =>
      {
        try
        {
          ProvisionExtensionCommand.NormalizeNumber(extNr);
        }
        catch (ArgumentOutOfRangeException ex)
        {
          context.AddFailure(nameof(ProvisionExtensionCommand.ExtNr), ex.Message);
        }
        catch (ArgumentException ex)
        {
          context.AddFailure(nameof(ProvisionExtensionCommand.ExtNr), ex.Message);
        }
      });
  }
}
