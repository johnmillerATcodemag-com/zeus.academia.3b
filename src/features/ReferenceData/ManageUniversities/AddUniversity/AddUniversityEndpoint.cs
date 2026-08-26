using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

public static class AddUniversityEndpoint
{
  public static RouteGroupBuilder MapAddUniversity(this RouteGroupBuilder group)
  {
    group.MapPost("/", async (AddUniversityCommand command, IValidator<AddUniversityCommand> validator, ISender sender, CancellationToken ct) =>
    {
      var validationResult = await validator.ValidateAsync(command, ct);
      if (!validationResult.IsValid)
      {
        return Results.ValidationProblem(validationResult.Errors
          .GroupBy(error => error.PropertyName)
          .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(error => error.ErrorMessage).ToArray()));
      }

      try
      {
        var response = await sender.Send(command, ct);
        return Results.Created($"/api/reference-data/universities/{response.Code}", response);
      }
      catch (ArgumentException ex) when (ex.ParamName == nameof(AddUniversityCommand.Code))
      {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
          [nameof(AddUniversityCommand.Code)] = [ex.Message]
        });
      }
      catch (UniversityConflictException ex)
      {
        return Results.Conflict(new { error = ex.Message });
      }
    })
    .WithName("AddUniversity")
    .Produces<AddUniversityResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status409Conflict)
    .ProducesValidationProblem();

    return group;
  }
}