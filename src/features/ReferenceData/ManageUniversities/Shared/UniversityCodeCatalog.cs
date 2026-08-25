using System.Collections.ObjectModel;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

public static class UniversityCodeCatalog
{
  private static readonly ReadOnlyDictionary<string, string> SupportedUniversitiesByCode =
    new(new Dictionary<string, string>(StringComparer.Ordinal)
    {
      ["BOSTON_U"] = "Boston University",
      ["MIT"] = "Massachusetts Institute of Technology",
      ["STANFORD"] = "Stanford University"
    });

  private static readonly ReadOnlyCollection<string> SupportedCodesCollection =
    Array.AsReadOnly(SupportedUniversitiesByCode.Keys.OrderBy(code => code, StringComparer.Ordinal).ToArray());

  public static IReadOnlyList<string> SupportedCodes => SupportedCodesCollection;

  public static string AllowedValuesMessage => string.Join(", ", SupportedCodesCollection);

  public static bool IsAllowed(string? code, out string normalizedCode)
  {
    normalizedCode = string.Empty;

    if (!TryParseUniversity(code, out var university))
    {
      return false;
    }

    normalizedCode = university.Code;
    return SupportedUniversitiesByCode.ContainsKey(normalizedCode);
  }

  public static bool TryResolve(string? code, out University university, out string universityName)
  {
    university = null!;
    universityName = string.Empty;

    if (!TryParseUniversity(code, out university))
    {
      return false;
    }

    if (!SupportedUniversitiesByCode.TryGetValue(university.Code, out var resolvedName))
    {
      return false;
    }

    universityName = resolvedName;
    return true;
  }

  private static bool TryParseUniversity(string? code, out University university)
  {
    university = null!;

    if (string.IsNullOrWhiteSpace(code))
    {
      return false;
    }

    try
    {
      university = University.Create(code);
      return true;
    }
    catch (ArgumentException)
    {
      return false;
    }
  }
}
