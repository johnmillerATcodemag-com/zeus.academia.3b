using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionCommandValidatorTests
{
    private readonly ProvisionExtensionCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenNumberIsNotPositive_ReturnsPositiveWholeNumberMessage(decimal number)
    {
        var command = new ProvisionExtensionCommand(number);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        var failure = Assert.Single(result.Errors);
        Assert.Equal(nameof(ProvisionExtensionCommand.Number), failure.PropertyName);
        Assert.Contains("positive whole number", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1.5)]
    [InlineData(12.25)]
    public void Validate_WhenNumberIsFractional_ReturnsWholeNumberMessage(decimal number)
    {
        var command = new ProvisionExtensionCommand(number);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        var failure = Assert.Single(result.Errors);
        Assert.Equal(nameof(ProvisionExtensionCommand.Number), failure.PropertyName);
        Assert.Contains("whole number", failure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(123)]
    [InlineData(10000)]
    public void Validate_WhenNumberIsPositiveWholeNumber_IsValid(decimal number)
    {
        var command = new ProvisionExtensionCommand(number);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
