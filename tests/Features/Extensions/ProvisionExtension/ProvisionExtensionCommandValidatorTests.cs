using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionCommandValidatorTests
{
   private readonly ProvisionExtensionCommandValidator _validator = new();

   [Theory]
   [InlineData(1)]
   [InlineData(42)]
   [InlineData(3000)]
   public void Validate_WhenNumberIsPositiveWholeValue_IsValid(decimal extNr)
   {
      var command = new ProvisionExtensionCommand(extNr);

      var result = _validator.Validate(command);

      Assert.True(result.IsValid);
   }

   [Theory]
   [InlineData(0)]
   [InlineData(-10)]
   public void Validate_WhenNumberIsNotPositive_IsInvalid(decimal extNr)
   {
      var command = new ProvisionExtensionCommand(extNr);

      var result = _validator.Validate(command);

      Assert.False(result.IsValid);
      var failure = Assert.Single(result.Errors);
      Assert.Equal(nameof(ProvisionExtensionCommand.ExtNr), failure.PropertyName);
      Assert.Contains("greater than zero", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
   }

   [Fact]
   public void Validate_WhenNumberExceedsIntMaxValue_IsInvalid()
   {
      var command = new ProvisionExtensionCommand(2147483648m);

      var result = _validator.Validate(command);

      Assert.False(result.IsValid);
      var failure = Assert.Single(result.Errors);
      Assert.Equal(nameof(ProvisionExtensionCommand.ExtNr), failure.PropertyName);
      Assert.Contains("between 1 and", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
   }

   [Theory]
   [InlineData(1.5)]
   [InlineData(10.25)]
   public void Validate_WhenNumberIsFractional_IsInvalid(decimal extNr)
   {
      var command = new ProvisionExtensionCommand(extNr);

      var result = _validator.Validate(command);

      Assert.False(result.IsValid);
      var failure = Assert.Single(result.Errors);
      Assert.Equal(nameof(ProvisionExtensionCommand.ExtNr), failure.PropertyName);
      Assert.Contains("whole number", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
   }
}
