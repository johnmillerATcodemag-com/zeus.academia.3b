using Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class DeprovisionExtensionCommandValidatorTests
{
  private readonly DeprovisionExtensionCommandValidator _validator = new();

  [Theory]
  [InlineData(1)]
  [InlineData(42)]
  public void Validate_WhenNumberIsPositive_IsValid(int number)
  {
    var command = new DeprovisionExtensionCommand(number);

    var result = _validator.Validate(command);

    Assert.True(result.IsValid);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Validate_WhenNumberIsNotPositive_ReturnsFailure(int number)
  {
    var command = new DeprovisionExtensionCommand(number);

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    var failure = Assert.Single(result.Errors);
    Assert.Equal(nameof(DeprovisionExtensionCommand.Number), failure.PropertyName);
    Assert.Contains("greater than zero", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
  }
}
