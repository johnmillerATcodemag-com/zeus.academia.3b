namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;

public sealed class ExtensionNotFoundException : Exception
{
  public ExtensionNotFoundException(string message)
    : base(message)
  {
  }
}
