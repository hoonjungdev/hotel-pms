using FluentValidation;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Housekeeping.MarkRoomClean;

internal static class MarkRoomCleanEndpoint
{
    public static RouteGroupBuilder MapMarkRoomCleanEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/rooms/{roomId:guid}/mark-clean", MarkRoomCleanAsync)
            .WithName("MarkRoomClean");

        return group;
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> MarkRoomCleanAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid roomId,
        MarkRoomCleanHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            MarkRoomCleanResult? result = await handler.HandleAsync(
                new MarkRoomCleanCommand(new TenantId(tenantId), new RoomId(roomId)),
                cancellationToken);

            return result is not null
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        }
        catch (ValidationException exception)
        {
            return TypedResults.ValidationProblem(ValidationProblemMapper.ToErrors(exception, "Room"));
        }
    }
}
