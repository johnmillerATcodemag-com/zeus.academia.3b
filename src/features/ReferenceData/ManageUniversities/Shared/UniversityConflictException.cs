namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

public sealed class UniversityConflictException : Exception
{
  public UniversityConflictException(string message)
    : base(message)
  {
  }
}
