namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

public static class ExtensionNumberNormalizer
{
   public const int MinValue = 1;

   public static int Normalize(decimal number)
   {
     if (number <= 0)
     {
       throw new ArgumentOutOfRangeException(nameof(number), "Extension number must be a positive whole number.");
     }

     if (number != decimal.Truncate(number))
     {
       throw new ArgumentException("Extension number must be a whole number.", nameof(number));
     }

     if (number > int.MaxValue)
     {
       throw new ArgumentOutOfRangeException(
         nameof(number),
         number,
         $"Extension number must be between {MinValue} and {int.MaxValue}.");
     }

     return (int)number;
   }

   public static int Normalize(int number)
   {
     if (number <= 0)
     {
       throw new ArgumentOutOfRangeException(nameof(number), "Extension number must be a positive whole number.");
     }

     return number;
   }
}
