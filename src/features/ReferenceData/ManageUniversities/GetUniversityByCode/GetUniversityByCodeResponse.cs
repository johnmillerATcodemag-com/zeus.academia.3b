namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.GetUniversityByCode;

public sealed record GetUniversityByCodeResponse(
  bool IsFound,
  string? Code,
  string? Name,
  bool IsActive);
