namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

public static class ExtensionNumberNormalizer
{
  public static int Normalize(decimal value, string paramName)
  {
    if (value != decimal.Truncate(value))
    {
      throw new ArgumentException("Extension number must be a whole number.", paramName);
    }

    if (value <= 0)
    {
      throw new ArgumentOutOfRangeException(paramName, value, "Extension number must be greater than zero.");
    }

    if (value > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(paramName, value, $"Extension number must be less than or equal to {int.MaxValue}.");
    }

    return (int)value;
  }

  public static bool TryNormalize(decimal value, out int normalized)
  {
    try
    {
      normalized = Normalize(value, nameof(value));
      return true;
    }
    catch (ArgumentException)
    {
      normalized = default;
      return false;
    }
    catch (OverflowException)
    {
      normalized = default;
      return false;
    }
  }
}
