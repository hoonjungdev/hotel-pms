using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Rooms.GetRoom;

internal static class GetRoomEndpoint
{
    public static RouteGroupBuilder MapGetRoomEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{roomId:guid}", GetRoomAsync)
            .WithName("GetRoom");

        return group;
    }

    private static async Task<Results<Ok<RoomResponse>, NotFound>> GetRoomAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid roomId,
        GetRoomHandler handler,
        CancellationToken cancellationToken)
    {
        RoomDetails? room = await handler.HandleAsync(
            new GetRoomQuery(new TenantId(tenantId), new RoomId(roomId)),
            cancellationToken);

        return room is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ToResponse(room));
    }

    private static RoomResponse ToResponse(RoomDetails room)
    {
        return new RoomResponse(room.Id.Value, room.Number, room.Condition);
    }
}
