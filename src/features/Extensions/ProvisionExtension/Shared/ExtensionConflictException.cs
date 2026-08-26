namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;

public sealed class ExtensionConflictException : Exception
{
  public ExtensionConflictException(string message)
    : base(message)
  {
  }
}
