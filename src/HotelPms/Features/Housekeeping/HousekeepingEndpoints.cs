using HotelPms.Features.Housekeeping.ListHousekeepingRooms;
using HotelPms.Features.Housekeeping.MarkRoomClean;

namespace HotelPms.Features.Housekeeping;

public static class HousekeepingEndpoints
{
    public static IEndpointRouteBuilder MapHousekeepingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/housekeeping")
            .WithTags("Housekeeping");

        group.MapListHousekeepingRoomsEndpoint();
        group.MapMarkRoomCleanEndpoint();

        return endpoints;
    }
}
