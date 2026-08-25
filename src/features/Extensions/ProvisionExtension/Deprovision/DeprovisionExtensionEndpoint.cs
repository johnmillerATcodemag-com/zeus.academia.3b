using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zeus.Academia.Features.SharedKernel.Foundation.Exceptions;

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Deprovision;

public static class DeprovisionExtensionEndpoint
{
    public static RouteGroupBuilder MapDeprovisionExtension(this RouteGroupBuilder group)
    {
        group.MapDelete("/{number}", async (decimal number, ISender sender, CancellationToken ct) =>
        {
            var command = new DeprovisionExtensionCommand(number);

            try
            {
                var response = await sender.Send(command, ct);
                return response.WasRemoved
                    ? Results.Ok(response)
                    : Results.NotFound(new { error = $"Extension {response.Number} was not found." });
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
