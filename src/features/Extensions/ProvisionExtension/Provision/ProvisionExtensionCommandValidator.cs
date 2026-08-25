using FluentValidation;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public sealed class ProvisionExtensionCommandValidator : AbstractValidator<ProvisionExtensionCommand>
{
   public ProvisionExtensionCommandValidator()
   {
     RuleFor(x => x.Number)
       .Cascade(CascadeMode.Stop)
       .Custom((value, context) =>
       {
         try
         {
           _ = ExtensionNumberNormalizer.Normalize(value);
         }
         catch (ArgumentOutOfRangeException ex)
         {
           context.AddFailure(nameof(ProvisionExtensionCommand.Number), ex.Message);
         }
         catch (ArgumentException ex)
         {
           context.AddFailure(nameof(ProvisionExtensionCommand.Number), ex.Message);
         }
       });
   }
}
