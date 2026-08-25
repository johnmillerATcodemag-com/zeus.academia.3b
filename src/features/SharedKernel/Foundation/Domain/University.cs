namespace Zeus.Academia.Features.SharedKernel.Foundation.Domain;

public sealed record University
{
  private University(string code)
  {
    Code = code;
  }

  public string Code { get; }

  public static University Create(string code)
  {
    var normalized = Normalize(code);
    if (normalized.Length > SharedKernelFieldLengths.UniversityCode)
    {
      throw new ArgumentException($"University code cannot exceed {SharedKernelFieldLengths.UniversityCode} characters.", nameof(code));
    }

    return new University(normalized);
  }

  internal static string Normalize(string code)
  {
    if (string.IsNullOrWhiteSpace(code))
    {
      throw new ArgumentException("University code is required.", nameof(code));
    }

    return code.Trim().ToUpperInvariant();
  }
}
