using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Rooms.ListRooms;

internal static class ListRoomsEndpoint
{
    public static RouteGroupBuilder MapListRoomsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListRoomsAsync)
            .WithName("ListRooms");

        return group;
    }

    private static async Task<Ok<IReadOnlyList<RoomResponse>>> ListRoomsAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        ListRoomsHandler handler,
        CancellationToken cancellationToken)
    {
        List<RoomListItem> rooms = await handler.HandleAsync(
            new ListRoomsQuery(new TenantId(tenantId)),
            cancellationToken);

        IReadOnlyList<RoomResponse> response = rooms
            .Select(ToResponse)
            .ToList();

        return TypedResults.Ok(response);
    }

    private static RoomResponse ToResponse(RoomListItem room)
    {
        return new RoomResponse(room.Id.Value, room.Number, room.Condition);
    }
}
