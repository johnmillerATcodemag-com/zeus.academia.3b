using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Provision;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

public static class ProvisionExtensionEndpoint
{
   public static RouteGroupBuilder MapProvisionExtension(this RouteGroupBuilder group)
   {
     group.MapPost("/", async (ProvisionExtensionCommand command, ISender sender, CancellationToken ct) =>
     {
       try
       {
         var response = await sender.Send(command, ct);
         return Results.Created($"/api/reference-data/extensions/{response.Number}", response);
       }
       catch (ConflictException ex)
       {
         return Results.Conflict(new { error = ex.Message });
       }
     })
     .WithName("ProvisionExtension")
     .Produces<ProvisionExtensionResponse>(StatusCodes.Status201Created)
     .Produces(StatusCodes.Status409Conflict)
     .ProducesValidationProblem();

     return group;
   }
}
