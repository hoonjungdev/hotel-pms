using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.RoomTypes.ListRoomTypes;

internal static class ListRoomTypesEndpoint
{
    public static RouteGroupBuilder MapListRoomTypesEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListRoomTypesAsync)
            .WithName("ListRoomTypes");

        return group;
    }

    private static async Task<Ok<IReadOnlyList<RoomTypeResponse>>> ListRoomTypesAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        ListRoomTypesHandler handler,
        CancellationToken cancellationToken)
    {
        List<RoomTypeListItem> roomTypes = await handler.HandleAsync(
            new ListRoomTypesQuery(new TenantId(tenantId)),
            cancellationToken);

        IReadOnlyList<RoomTypeResponse> response = roomTypes
            .Select(ToResponse)
            .ToList();

        return TypedResults.Ok(response);
    }

    private static RoomTypeResponse ToResponse(RoomTypeListItem roomType)
    {
        return new RoomTypeResponse(
            roomType.Id.Value,
            roomType.Code,
            roomType.Name,
            roomType.BaseOccupancy,
            roomType.MaxOccupancy);
    }
}
