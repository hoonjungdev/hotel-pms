using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Housekeeping.ListHousekeepingRooms;

internal static class ListHousekeepingRoomsEndpoint
{
    public static RouteGroupBuilder MapListHousekeepingRoomsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/rooms", ListHousekeepingRoomsAsync)
            .WithName("ListHousekeepingRooms");

        return group;
    }

    private static async Task<Results<Ok<IReadOnlyList<HousekeepingRoomResponse>>, ValidationProblem>> ListHousekeepingRoomsAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        [FromQuery] string? condition,
        ListHousekeepingRoomsHandler handler,
        CancellationToken cancellationToken)
    {
        RoomCondition? parsedCondition = null;

        if (!string.IsNullOrWhiteSpace(condition))
        {
            if (!Enum.TryParse(condition, ignoreCase: true, out RoomCondition roomCondition))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(condition)] = ["Unsupported room condition."]
                });
            }

            parsedCondition = roomCondition;
        }

        List<HousekeepingRoomListItem> rooms = await handler.HandleAsync(
            new ListHousekeepingRoomsQuery(new TenantId(tenantId), parsedCondition),
            cancellationToken);

        return TypedResults.Ok<IReadOnlyList<HousekeepingRoomResponse>>(
            rooms.Select(room => new HousekeepingRoomResponse(
                    room.Id.Value,
                    room.RoomTypeId.Value,
                    room.Number,
                    room.Condition))
                .ToList());
    }
}
