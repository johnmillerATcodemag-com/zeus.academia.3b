using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionCommandValidatorTests
{
  private readonly ProvisionExtensionCommandValidator _validator = new();

  [Theory]
  [InlineData(1)]
  [InlineData(7.00)]
  [InlineData(42)]
  public void Validate_WhenExtNrIsPositiveWholeNumber_IsValid(decimal extNr)
  {
    var command = new ProvisionExtensionCommand(extNr);

    var result = _validator.Validate(command);

    Assert.True(result.IsValid);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Validate_WhenExtNrIsNotPositive_ReturnsPositiveFailure(decimal extNr)
  {
    var command = new ProvisionExtensionCommand(extNr);

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    var failure = Assert.Single(result.Errors);
    Assert.Equal(nameof(ProvisionExtensionCommand.ExtNr), failure.PropertyName);
    Assert.Contains("greater than zero", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
  }

  [Theory]
  [InlineData(1.5)]
  [InlineData(99.25)]
  public void Validate_WhenExtNrIsFractional_ReturnsWholeNumberFailure(decimal extNr)
  {
    var command = new ProvisionExtensionCommand(extNr);

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    var failure = Assert.Single(result.Errors);
    Assert.Equal(nameof(ProvisionExtensionCommand.ExtNr), failure.PropertyName);
    Assert.Contains("whole number", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
  }

  [Theory]
  [InlineData(2147483648)]
  [InlineData(999999999999)]
  public void Validate_WhenExtNrIsOutOfRange_ReturnsRangeFailure(decimal extNr)
  {
    var command = new ProvisionExtensionCommand(extNr);

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    var failure = Assert.Single(result.Errors);
    Assert.Equal(nameof(ProvisionExtensionCommand.ExtNr), failure.PropertyName);
    Assert.Contains("between 1 and", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
  }
}
