using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.SharedKernel.Foundation.Domain;

public sealed class Extension
{
  private Extension()
  {
  }

  private Extension(int number)
  {
    Number = number;
  }

  public int Number { get; private set; }

  public string? AssignedEmpNr { get; private set; }

  public bool IsAvailable => AssignedEmpNr is null;

  public static Extension Create(int number)
  {
    if (number <= 0)
    {
      throw new ArgumentException("Extension number must be positive.", nameof(number));
    }

    return new Extension(number);
  }

  public static Extension Create(decimal number)
  {
    if (number <= 0m)
    {
      throw new ArgumentOutOfRangeException(nameof(number), "Extension number must be greater than zero.");
    }

    if (number != decimal.Truncate(number))
    {
      throw new ArgumentException(
        "Extension number must be a whole number. Fractional values are not allowed.",
        nameof(number));
    }

    if (number > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(
        nameof(number),
        $"Extension number must be between 1 and {int.MaxValue}.");
    }

    return Create((int)number);
  }

  public void AssignTo(string empNr)
  {
    var normalizedEmpNr = Academic.NormalizeEmpNr(empNr);

    if (AssignedEmpNr is not null && !string.Equals(AssignedEmpNr, normalizedEmpNr, StringComparison.Ordinal))
    {
      throw new ConflictException("Extension is already assigned to a different academic.");
    }

    AssignedEmpNr = normalizedEmpNr;
  }

  public void ReleaseFrom(string empNr)
  {
    var normalizedEmpNr = Academic.NormalizeEmpNr(empNr);

    if (AssignedEmpNr is null)
    {
      return;
    }

    if (!string.Equals(AssignedEmpNr, normalizedEmpNr, StringComparison.Ordinal))
    {
      throw new ConflictException("Cannot release extension from a different academic.");
    }

    AssignedEmpNr = null;
  }
}
