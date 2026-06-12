using HotelPms.Features.Reservations.CancelReservation;
using HotelPms.Features.Reservations.CheckInReservation;
using HotelPms.Features.Reservations.CheckOutReservation;
using HotelPms.Features.Reservations.CheckReservationAvailability;
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
        group.MapCheckReservationAvailabilityEndpoint();
        group.MapGetReservationEndpoint();
        group.MapCreateReservationEndpoint();
        group.MapConfirmReservationEndpoint();
        group.MapCancelReservationEndpoint();
        group.MapCheckInReservationEndpoint();
        group.MapCheckOutReservationEndpoint();

        return endpoints;
    }
}
