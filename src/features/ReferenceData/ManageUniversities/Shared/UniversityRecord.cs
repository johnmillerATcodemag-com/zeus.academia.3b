using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

public sealed class UniversityRecord
{
  public string Code { get; private set; } = string.Empty;

  public string Name { get; private set; } = string.Empty;

  public bool IsActive { get; private set; } = true;

  public static UniversityRecord Create(string code, string name)
  {
    var normalizedCode = University.Create(code).Code;

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

    return new UniversityRecord
    {
      Code = normalizedCode,
      Name = normalizedName,
      IsActive = true
    };
  }

  public void Deactivate() => IsActive = false;

  public void Reactivate() => IsActive = true;
}