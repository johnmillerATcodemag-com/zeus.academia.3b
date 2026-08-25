using FluentValidation;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed class ProvisionExtensionCommandValidator : AbstractValidator<ProvisionExtensionCommand>
{
   public ProvisionExtensionCommandValidator()
   {
      RuleFor(x => x.ExtNr)
         .Cascade(CascadeMode.Stop)
         .Must(value => value > 0)
         .WithMessage("Extension number must be greater than zero.")
         .Must(value => value == decimal.Truncate(value))
         .WithMessage("Extension number must be a whole number.")
         .Must(value => value >= 1 && value <= int.MaxValue)
         .WithMessage($"Extension number must be between 1 and {int.MaxValue}.");
   }
}
