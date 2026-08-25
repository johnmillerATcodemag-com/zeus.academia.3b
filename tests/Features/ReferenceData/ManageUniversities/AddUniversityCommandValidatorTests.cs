using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

public sealed class AddUniversityCommandValidatorTests
{
  private readonly AddUniversityCommandValidator _validator = new();

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void Validate_WhenCodeMissing_ReturnsRequiredMessage(string? code)
  {
    var command = new AddUniversityCommand(code!);

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    var failure = Assert.Single(result.Errors);
    Assert.Equal(nameof(AddUniversityCommand.Code), failure.PropertyName);
    Assert.Equal("Code is required.", failure.ErrorMessage);
  }

  [Fact]
  public void Validate_WhenCodeExceedsCanonicalLength_ReturnsLengthMessage()
  {
    var command = new AddUniversityCommand(new string('X', SharedKernelFieldLengths.UniversityCode + 1));

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    var failure = Assert.Single(result.Errors);
    Assert.Equal(nameof(AddUniversityCommand.Code), failure.PropertyName);
    Assert.Contains($"cannot exceed {SharedKernelFieldLengths.UniversityCode}", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
  }

  [Theory]
  [InlineData("OXFORD")]
  [InlineData("unknown_u")]
  public void Validate_WhenCodeInvalid_ReturnsAllowedValuesMessage(string code)
  {
    var command = new AddUniversityCommand(code);

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    var failure = Assert.Single(result.Errors);
    Assert.Equal(nameof(AddUniversityCommand.Code), failure.PropertyName);
    Assert.Contains("Allowed values", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    Assert.Contains(UniversityCodeCatalog.AllowedValuesMessage, failure.ErrorMessage, StringComparison.Ordinal);
  }

  [Theory]
  [InlineData("MIT")]
  [InlineData("mit")]
  [InlineData(" boston_u ")]
  [InlineData("Stanford")]
  public void Validate_WhenCodeIsCanonicalAfterNormalization_IsValid(string code)
  {
    var command = new AddUniversityCommand(code);

    var result = _validator.Validate(command);

    Assert.True(result.IsValid);
  }
}