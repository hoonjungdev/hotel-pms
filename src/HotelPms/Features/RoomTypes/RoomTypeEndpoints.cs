using HotelPms.Features.RoomTypes.CreateRoomType;
using HotelPms.Features.RoomTypes.GetRoomType;
using HotelPms.Features.RoomTypes.ListRoomTypes;

namespace HotelPms.Features.RoomTypes;

public static class RoomTypeEndpoints
{
    public static IEndpointRouteBuilder MapRoomTypeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/room-types")
            .WithTags("Room Types");

        group.MapListRoomTypesEndpoint();
        group.MapGetRoomTypeEndpoint();
        group.MapCreateRoomTypeEndpoint();

        return endpoints;
    }
}
