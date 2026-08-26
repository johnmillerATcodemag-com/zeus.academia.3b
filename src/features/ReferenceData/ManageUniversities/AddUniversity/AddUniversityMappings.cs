namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

public static class AddUniversityMappings
{
  public static UniversityRecord ToUniversityRecord(this AddUniversityCommand command)
  {
    if (!UniversityCodeCatalog.TryResolveName(command.Code, out var normalizedCode, out var universityName))
    {
      throw new ArgumentException($"Allowed values: {UniversityCodeCatalog.AllowedValuesMessage}", nameof(command.Code));
    }

    return UniversityRecord.Create(normalizedCode, universityName);
  }

  public static AddUniversityResponse ToResponse(this UniversityRecord universityRecord)
  {
    return new AddUniversityResponse(universityRecord.Code, universityRecord.Name, universityRecord.IsActive);
  }
}