using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.ListUniversities;

public static class ListUniversitiesEndpoint
{
  public static RouteGroupBuilder MapListUniversities(this RouteGroupBuilder group)
  {
    group.MapGet("/", async (ISender sender, CancellationToken ct) =>
    {
      var response = await sender.Send(new ListUniversitiesQuery(), ct);
      return Results.Ok(response);
    })
    .WithName("ListUniversities")
    .Produces<IReadOnlyList<ListUniversitiesResponse>>(StatusCodes.Status200OK);

    return group;
  }
}