using System.Collections.ObjectModel;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

public static class UniversityCodeCatalog
{
  private static readonly ReadOnlyCollection<UniversityCatalogEntry> SupportedUniversitiesCollection =
    Array.AsReadOnly(
    [
      new UniversityCatalogEntry("BOSTON_U", "Boston University"),
      new UniversityCatalogEntry("HARVARD", "Harvard University"),
      new UniversityCatalogEntry("MIT", "Massachusetts Institute of Technology"),
      new UniversityCatalogEntry("PRINCETON", "Princeton University"),
      new UniversityCatalogEntry("STANFORD", "Stanford University"),
      new UniversityCatalogEntry("YALE", "Yale University")
    ]);

  public static IReadOnlyList<UniversityCatalogEntry> SupportedUniversities => SupportedUniversitiesCollection;

  public static bool IsAllowed(string? code, out string normalizedCode)
  {
    normalizedCode = NormalizeCode(code);
    var normalized = normalizedCode;
    return SupportedUniversitiesCollection.Any(x => x.Code == normalized);
  }

  public static bool TryParseUniversity(string? code, out UniversityCatalogEntry university)
  {
    university = new UniversityCatalogEntry(string.Empty, string.Empty);

    if (!IsAllowed(code, out var normalizedCode))
    {
      return false;
    }

    university = SupportedUniversitiesCollection.Single(x => x.Code == normalizedCode);
    return true;
  }

  public static string AllowedValuesMessage =>
    string.Join(", ", SupportedUniversitiesCollection.Select(x => x.Code));

  public static string NormalizeCode(string? code)
  {
    if (string.IsNullOrWhiteSpace(code))
    {
      return string.Empty;
    }

    var trimmedCode = code.Trim();
    if (trimmedCode.Length > SharedKernelFieldLengths.UniversityCode)
    {
      return trimmedCode.ToUpperInvariant();
    }

    return University.Create(trimmedCode).Code;
  }
}