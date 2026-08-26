using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Shared;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public static class DeprovisionExtensionEndpoint
{
  public static RouteHandlerBuilder MapDeprovisionExtension(this RouteGroupBuilder group)
  {
    return group.MapDelete("/{number:int}", async (
      int number,
      IValidator<DeprovisionExtensionCommand> validator,
      ISender sender,
      CancellationToken ct) =>
    {
      var command = new DeprovisionExtensionCommand(number);
      var validationResult = await validator.ValidateAsync(command, ct);
      if (!validationResult.IsValid)
      {
        return Results.ValidationProblem(ToDictionary(validationResult));
      }

      try
      {
        var response = await sender.Send(command, ct);
        return Results.Ok(response);
      }
      catch (ExtensionNotFoundException ex)
      {
        return Results.NotFound(new { error = ex.Message });
      }
      catch (ConflictException ex)
      {
        return Results.Conflict(new { error = ex.Message });
      }
    })
    .WithName("DeprovisionExtension")
    .Produces<DeprovisionExtensionResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
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
