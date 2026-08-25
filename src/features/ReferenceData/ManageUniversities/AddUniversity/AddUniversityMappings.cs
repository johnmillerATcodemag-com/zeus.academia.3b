using Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

public static class AddUniversityMappings
{
  public static UniversityRecord ToUniversityRecord(this AddUniversityCommand command)
  {
    if (!UniversityCodeCatalog.TryResolve(command.Code, out var university, out var universityName))
    {
      throw new ArgumentException($"Allowed values: {UniversityCodeCatalog.AllowedValuesMessage}", nameof(command.Code));
    }

    return UniversityRecord.Create(university.Code, universityName);
  }

  public static AddUniversityResponse ToResponse(this UniversityRecord universityRecord)
  {
    return new AddUniversityResponse(universityRecord.Code, universityRecord.Name, universityRecord.IsActive);
  }
}
