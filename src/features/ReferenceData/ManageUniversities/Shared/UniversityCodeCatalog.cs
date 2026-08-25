using System.Collections.ObjectModel;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

public sealed record UniversityCatalogEntry(string Code, string Name);

public static class UniversityCodeCatalog
{
  private static readonly ReadOnlyCollection<UniversityCatalogEntry> SupportedUniversitiesCollection =
    Array.AsReadOnly(
    [
      new UniversityCatalogEntry("BOSTON_U", "Boston University"),
      new UniversityCatalogEntry("HARVARD", "Harvard University"),
      new UniversityCatalogEntry("MIT", "Massachusetts Institute of Technology"),
      new UniversityCatalogEntry("STANFORD", "Stanford University")
    ]);

  private static readonly ReadOnlyCollection<string> SupportedCodesCollection =
    Array.AsReadOnly(SupportedUniversitiesCollection.Select(x => x.Code).ToArray());

  private static readonly IReadOnlyDictionary<string, UniversityCatalogEntry> SupportedUniversitiesByCode =
    SupportedUniversitiesCollection.ToDictionary(x => x.Code, StringComparer.Ordinal);

  public static IReadOnlyList<UniversityCatalogEntry> SupportedUniversities => SupportedUniversitiesCollection;

  public static IReadOnlyList<string> SupportedCodes => SupportedCodesCollection;

  public static string AllowedValuesMessage => string.Join(", ", SupportedCodesCollection);

  public static bool IsWithinCanonicalLength(string? code)
  {
    return string.IsNullOrWhiteSpace(code) || code.Trim().Length <= SharedKernelFieldLengths.UniversityCode;
  }

  public static bool IsAllowed(string? code, out string normalizedCode)
  {
    if (!TryNormalizeCode(code, out normalizedCode))
    {
      return false;
    }

    return SupportedUniversitiesByCode.ContainsKey(normalizedCode);
  }

  public static bool TryNormalizeCode(string? code, out string normalizedCode)
  {
    try
    {
      normalizedCode = University.Create(code ?? string.Empty).Code;
      return true;
    }
    catch (ArgumentException)
    {
      normalizedCode = string.Empty;
      return false;
    }
  }

  public static bool TryGetCanonicalEntry(string? code, out UniversityCatalogEntry entry)
  {
    entry = null!;

    if (!IsAllowed(code, out var normalizedCode))
    {
      return false;
    }

    if (SupportedUniversitiesByCode.TryGetValue(normalizedCode, out var catalogEntry))
    {
      entry = catalogEntry;
      return true;
    }

    return false;
  }
}