using FluentValidation;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionCommandValidatorTests
{
  private readonly ProvisionExtensionCommandValidator _sut = new();

  [Theory]
  [InlineData(1)]
  [InlineData(101)]
  [InlineData(2147483647)]
  public void Validate_WithPositiveWholeNumber_AllowsProvision(decimal number)
  {
    var result = _sut.Validate(new ProvisionExtensionCommand(number));

    Assert.True(result.IsValid);
  }

  [Fact]
  public void Validate_WithFractionalNumber_RejectsProvision()
  {
    var result = _sut.Validate(new ProvisionExtensionCommand(12.5m));

    Assert.False(result.IsValid);
    var failure = Assert.Single(result.Errors);
    Assert.Equal(nameof(ProvisionExtensionCommand.Number), failure.PropertyName);
    Assert.Contains("whole number", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Validate_WithOutOfRangeValues_RejectsProvision()
  {
    foreach (var number in new decimal[] { 0m, -1m, 2147483648m })
    {
      var result = _sut.Validate(new ProvisionExtensionCommand(number));

      Assert.False(result.IsValid);
      var failure = Assert.Single(result.Errors);
      Assert.Equal(nameof(ProvisionExtensionCommand.Number), failure.PropertyName);
      Assert.Contains("positive whole number", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
  }
}