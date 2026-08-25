using FluentValidation;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed class DeprovisionExtensionCommandValidator : AbstractValidator<DeprovisionExtensionCommand>
{
    public DeprovisionExtensionCommandValidator()
    {
        RuleFor(x => x.Number)
            .Cascade(CascadeMode.Stop)
            .Must(number => number > 0)
            .WithMessage("Extension number must be a positive whole number.")
            .Must(number => number == decimal.Truncate(number))
            .WithMessage("Extension number must be a whole number; fractional values are not allowed.")
            .Must(number => number <= int.MaxValue)
            .WithMessage($"Extension number must be between 1 and {int.MaxValue}.");
    }
}
