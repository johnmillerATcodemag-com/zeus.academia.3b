using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionCommandValidatorTests
{
   private readonly ProvisionExtensionCommandValidator _validator = new();

   [Theory]
   [InlineData(1)]
   [InlineData(100)]
   [InlineData(2147483647)]
   public void Validate_WhenNumberIsPositiveWholeValue_IsValid(int value)
   {
     var command = new ProvisionExtensionCommand(value);

     var result = _validator.Validate(command);

     Assert.True(result.IsValid);
   }

   [Theory]
   [InlineData(0.5)]
   [InlineData(1.5)]
   [InlineData(10.25)]
   public void Validate_WhenNumberIsFractional_ReturnsWholeNumberFailure(decimal value)
   {
     var command = new ProvisionExtensionCommand(value);

     var result = _validator.Validate(command);

     Assert.False(result.IsValid);
     var failure = Assert.Single(result.Errors);
     Assert.Equal(nameof(ProvisionExtensionCommand.Number), failure.PropertyName);
     Assert.Contains("whole number", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
   }

   [Theory]
   [InlineData(0)]
   [InlineData(-1)]
   public void Validate_WhenNumberIsNotPositive_ReturnsPositiveFailure(decimal value)
   {
     var command = new ProvisionExtensionCommand(value);

     var result = _validator.Validate(command);

     Assert.False(result.IsValid);
     var failure = Assert.Single(result.Errors);
     Assert.Equal(nameof(ProvisionExtensionCommand.Number), failure.PropertyName);
     Assert.Contains("positive", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
   }

   [Fact]
   public void Validate_WhenNumberExceedsIntRange_ReturnsRangeFailure()
   {
     var command = new ProvisionExtensionCommand(2147483648m);

     var result = _validator.Validate(command);

     Assert.False(result.IsValid);
     var failure = Assert.Single(result.Errors);
     Assert.Equal(nameof(ProvisionExtensionCommand.Number), failure.PropertyName);
     Assert.Contains("between 1 and 2147483647", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
   }
}
