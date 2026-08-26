using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.ListUniversities;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

public static class ManageUniversitiesEndpoints
{
  public static IEndpointRouteBuilder MapManageUniversitiesEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/reference-data/universities");
    group.MapAddUniversity();
    group.MapListUniversities();
    return app;
  }
}