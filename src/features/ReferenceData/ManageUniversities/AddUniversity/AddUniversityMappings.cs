using Zeus.Academia.Features.ReferenceData.ManageUniversities;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

public static class AddUniversityMappings
{
  public static UniversityRecord ToUniversityRecord(this AddUniversityCommand command)
  {
    if (!UniversityCodeCatalog.TryGetCanonicalEntry(command.Code, out var entry))
    {
      throw new ArgumentException($"Allowed values: {UniversityCodeCatalog.AllowedValuesMessage}", nameof(command.Code));
    }

    return UniversityRecord.Create(entry.Code, entry.Name);
  }

  public static AddUniversityResponse ToResponse(this UniversityRecord universityRecord)
  {
    return new AddUniversityResponse(universityRecord.Code, universityRecord.Name, universityRecord.IsActive);
  }
}