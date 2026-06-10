using HotelPms.Features.Rooms.AddRoom;
using HotelPms.Features.Rooms.GetRoom;
using HotelPms.Features.Rooms.ListRooms;
using HotelPms.Features.Rooms.UpdateRoomCondition;

namespace HotelPms.Features.Rooms;

public static class RoomEndpoints
{
    public static IEndpointRouteBuilder MapRoomEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/rooms")
            .WithTags("Rooms");

        group.MapListRoomsEndpoint();
        group.MapGetRoomEndpoint();
        group.MapAddRoomEndpoint();
        group.MapUpdateRoomConditionEndpoint();

        return endpoints;
    }
}
