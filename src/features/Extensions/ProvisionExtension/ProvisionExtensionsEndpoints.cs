using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

public static class ProvisionExtensionsEndpoints
{
   public static IEndpointRouteBuilder MapProvisionExtensionsEndpoints(this IEndpointRouteBuilder app)
   {
      var group = app.MapGroup("/api/reference-data/extensions");
      group.MapProvisionExtension();
      group.MapDeprovisionExtension();
      return app;
   }
}
