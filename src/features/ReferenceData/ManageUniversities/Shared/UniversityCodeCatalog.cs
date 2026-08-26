using System.Collections.ObjectModel;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

public static class UniversityCodeCatalog
{
  private static readonly ReadOnlyDictionary<string, string> UniversitiesByCode =
    new(new Dictionary<string, string>(StringComparer.Ordinal)
    {
      ["BOSTON_U"] = "Boston University",
      ["MIT"] = "Massachusetts Institute of Technology",
      ["STANFORD"] = "Stanford University"
    });

  private static readonly ReadOnlyCollection<string> SupportedCodesCollection =
    Array.AsReadOnly(UniversitiesByCode.Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray());

  public static IReadOnlyList<string> SupportedCodes => SupportedCodesCollection;

  public static string AllowedValuesMessage => string.Join(", ", SupportedCodesCollection);

  public static bool IsAllowed(string? code, out string normalizedCode)
  {
    if (!TryNormalizeCode(code, out normalizedCode))
    {
      return false;
    }

    return UniversitiesByCode.ContainsKey(normalizedCode);
  }

  public static bool TryResolveName(string? code, out string normalizedCode, out string universityName)
  {
    universityName = string.Empty;

    if (!IsAllowed(code, out normalizedCode))
    {
      return false;
    }

    universityName = UniversitiesByCode[normalizedCode];
    return true;
  }

  private static bool TryNormalizeCode(string? code, out string normalizedCode)
  {
    normalizedCode = string.Empty;

    if (string.IsNullOrWhiteSpace(code))
    {
      return false;
    }

    try
    {
      normalizedCode = University.Create(code).Code;
      return true;
    }
    catch (ArgumentException)
    {
      return false;
    }
  }
}