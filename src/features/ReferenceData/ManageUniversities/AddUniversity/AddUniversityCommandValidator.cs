using FluentValidation;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

public sealed class AddUniversityCommandValidator : AbstractValidator<AddUniversityCommand>
{
  public AddUniversityCommandValidator()
  {
    RuleFor(x => x.Code)
      .Cascade(CascadeMode.Stop)
      .Must(code => !string.IsNullOrWhiteSpace(code))
      .WithMessage("Code is required.")
      .Must(UniversityCodeCatalog.IsWithinCanonicalLength)
      .WithMessage($"Code cannot exceed {SharedKernelFieldLengths.UniversityCode} characters.")
      .Must(code => UniversityCodeCatalog.IsAllowed(code, out _))
      .WithMessage(_ => $"Allowed values: {UniversityCodeCatalog.AllowedValuesMessage}");
  }
}