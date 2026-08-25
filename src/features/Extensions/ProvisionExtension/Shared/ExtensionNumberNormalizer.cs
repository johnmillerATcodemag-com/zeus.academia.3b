namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

public static class ExtensionNumberNormalizer
{
    public static int Normalize(decimal number)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), number, "Extension number must be a positive whole number.");
        }

        if (number != decimal.Truncate(number))
        {
            throw new ArgumentException("Extension number must be a whole number; fractional values are not allowed.", nameof(number));
        }

        if (number > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(number), number, $"Extension number must be between 1 and {int.MaxValue}.");
        }

        return (int)number;
    }

    public static bool TryNormalize(decimal number, out int normalized)
    {
        try
        {
            normalized = Normalize(number);
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
