using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.SharedKernel.Foundation;

public sealed class ExtensionTests
{
  [Fact]
  public void Create_WithWholeDecimal_ReturnsExactNumber()
  {
    var extension = Extension.Create(42m);

    Assert.Equal(42, extension.Number);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Create_WithNonPositiveDecimal_ThrowsRangeException(decimal number)
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Extension.Create(number));

    Assert.Contains("greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Create_WithFractionalDecimal_RejectsLossyCoercion()
  {
    var exception = Assert.Throws<ArgumentException>(() => Extension.Create(42.5m));

    Assert.Contains("whole number", exception.Message, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Create_AboveInt32Range_ThrowsRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Extension.Create((decimal)int.MaxValue + 1));

    Assert.Contains("between 1 and", exception.Message, StringComparison.OrdinalIgnoreCase);
  }
}
