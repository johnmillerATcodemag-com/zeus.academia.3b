using FluentValidation;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public sealed class DeprovisionExtensionCommandValidator : AbstractValidator<DeprovisionExtensionCommand>
{
   public DeprovisionExtensionCommandValidator()
   {
     RuleFor(x => x.Number)
       .Custom((value, context) =>
       {
         try
         {
           _ = ExtensionNumberNormalizer.Normalize(value);
         }
         catch (ArgumentOutOfRangeException ex)
         {
           context.AddFailure(nameof(DeprovisionExtensionCommand.Number), ex.Message);
         }
       });
   }
}
