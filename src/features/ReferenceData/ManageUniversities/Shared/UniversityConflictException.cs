namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

public sealed class UniversityConflictException : Exception
{
  public UniversityConflictException(string message)
    : base(message)
  {
  }
}