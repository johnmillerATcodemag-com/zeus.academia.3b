using FluentValidation;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

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
          Extension.Create(extNr);
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
