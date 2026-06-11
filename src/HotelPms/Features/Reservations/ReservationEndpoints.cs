using HotelPms.Features.Reservations.CancelReservation;
using HotelPms.Features.Reservations.ConfirmReservation;
using HotelPms.Features.Reservations.CreateReservation;
using HotelPms.Features.Reservations.GetReservation;
using HotelPms.Features.Reservations.ListReservations;

namespace HotelPms.Features.Reservations;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/reservations")
            .WithTags("Reservations");

        group.MapListReservationsEndpoint();
        group.MapGetReservationEndpoint();
        group.MapCreateReservationEndpoint();
        group.MapConfirmReservationEndpoint();
        group.MapCancelReservationEndpoint();

        return endpoints;
    }
}
