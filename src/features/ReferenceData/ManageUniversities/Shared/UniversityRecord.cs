using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

public sealed class UniversityRecord
{
  private UniversityRecord()
  {
  }

  private UniversityRecord(string code, string name, bool isActive)
  {
    Code = code;
    Name = name;
    IsActive = isActive;
  }

  public string Code { get; private set; } = string.Empty;

  public string Name { get; private set; } = string.Empty;

  public bool IsActive { get; private set; }

  public static UniversityRecord Create(string code, string name)
  {
    var university = University.Create(code);

    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("University name is required.", nameof(name));
    }

    var normalizedName = name.Trim();
    if (normalizedName.Length > SharedKernelFieldLengths.UniversityName)
    {
      throw new ArgumentException(
        $"University name cannot exceed {SharedKernelFieldLengths.UniversityName} characters.",
        nameof(name));
    }

    return new UniversityRecord(university.Code, normalizedName, isActive: true);
  }

  public void Deactivate() => IsActive = false;

  public void Reactivate() => IsActive = true;
}