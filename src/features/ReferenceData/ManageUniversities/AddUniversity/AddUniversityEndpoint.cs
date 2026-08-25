using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

public static class AddUniversityEndpoint
{
  public static RouteGroupBuilder MapAddUniversity(this RouteGroupBuilder group)
  {
    group.MapPost("/", async (AddUniversityCommand command, ISender sender, CancellationToken ct) =>
    {
      try
      {
        var response = await sender.Send(command, ct);
        return Results.Created($"/api/reference-data/universities/{response.Code}", response);
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