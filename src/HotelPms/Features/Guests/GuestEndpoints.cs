using HotelPms.Features.Guests.GetGuest;
using HotelPms.Features.Guests.ListGuests;
using HotelPms.Features.Guests.RegisterGuest;

namespace HotelPms.Features.Guests;

public static class GuestEndpoints
{
    public static IEndpointRouteBuilder MapGuestEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/guests")
            .WithTags("Guests");

        group.MapListGuestsEndpoint();
        group.MapGetGuestEndpoint();
        group.MapRegisterGuestEndpoint();

        return endpoints;
    }
}
