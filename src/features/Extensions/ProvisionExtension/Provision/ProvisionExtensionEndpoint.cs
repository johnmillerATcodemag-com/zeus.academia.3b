using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;

public static class ProvisionExtensionEndpoint
{
  public static RouteHandlerBuilder MapProvisionExtension(this RouteGroupBuilder group)
  {
    return group.MapPost("/", async (ProvisionExtensionCommand command, ISender sender, CancellationToken ct) =>
    {
      var validationResult = new ProvisionExtensionCommandValidator().Validate(command);
      if (!validationResult.IsValid)
      {
        return Results.ValidationProblem(ToDictionary(validationResult));
      }

      try
      {
        var response = await sender.Send(command, ct);
        return Results.Created($"/api/reference-data/extensions/{response.Number}", response);
      }
      catch (ExtensionConflictException ex)
      {
        return Results.Conflict(new { error = ex.Message });
      }
    })
    .WithName("ProvisionExtension")
    .Produces<ProvisionExtensionResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status409Conflict)
    .ProducesValidationProblem();
  }

  private static IDictionary<string, string[]> ToDictionary(ValidationResult validationResult)
  {
    return validationResult.Errors
      .GroupBy(x => x.PropertyName)
      .ToDictionary(
        x => x.Key,
        x => x.Select(y => y.ErrorMessage).ToArray());
  }
}
