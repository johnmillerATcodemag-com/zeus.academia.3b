using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

public static class DeprovisionExtensionEndpoint
{
   public static RouteGroupBuilder MapDeprovisionExtension(this RouteGroupBuilder group)
   {
     group.MapDelete("/{number:int}", async (int number, ISender sender, CancellationToken ct) =>
     {
       try
       {
         var response = await sender.Send(new DeprovisionExtensionCommand(number), ct);
         return Results.Ok(response);
       }
       catch (KeyNotFoundException ex)
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

     return group;
   }
}
