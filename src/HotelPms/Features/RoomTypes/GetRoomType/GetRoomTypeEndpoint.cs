using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.RoomTypes.GetRoomType;

internal static class GetRoomTypeEndpoint
{
    public static RouteGroupBuilder MapGetRoomTypeEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{roomTypeId:guid}", GetRoomTypeAsync)
            .WithName("GetRoomType");

        return group;
    }

    private static async Task<Results<Ok<RoomTypeResponse>, NotFound>> GetRoomTypeAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid roomTypeId,
        GetRoomTypeHandler handler,
        CancellationToken cancellationToken)
    {
        RoomTypeDetails? roomType = await handler.HandleAsync(
            new GetRoomTypeQuery(new TenantId(tenantId), new RoomTypeId(roomTypeId)),
            cancellationToken);

        return roomType is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ToResponse(roomType));
    }

    private static RoomTypeResponse ToResponse(RoomTypeDetails roomType)
    {
        return new RoomTypeResponse(
            roomType.Id.Value,
            roomType.Code,
            roomType.Name,
            roomType.BaseOccupancy,
            roomType.MaxOccupancy);
    }
}
