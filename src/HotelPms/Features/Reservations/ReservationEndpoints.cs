using HotelPms.Features.Reservations.CreateReservation;

namespace HotelPms.Features.Reservations;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/reservations")
            .WithTags("Reservations");

        group.MapCreateReservationEndpoint();

        return endpoints;
    }
}
