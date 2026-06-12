using HotelPms.Features.Reservations.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Reservations.GetReservation;

internal static class GetReservationEndpoint
{
    public static RouteGroupBuilder MapGetReservationEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{reservationId:guid}", GetReservationAsync)
            .WithName("GetReservation");

        return group;
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound>> GetReservationAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid reservationId,
        GetReservationHandler handler,
        CancellationToken cancellationToken)
    {
        ReservationDetails? reservation = await handler.HandleAsync(
            new GetReservationQuery(new TenantId(tenantId), new ReservationId(reservationId)),
            cancellationToken);

        return reservation is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ToResponse(reservation));
    }

    private static ReservationResponse ToResponse(ReservationDetails reservation)
    {
        return new ReservationResponse(
            reservation.Id.Value,
            reservation.PrimaryGuestId.Value,
            reservation.RoomTypeId.Value,
            reservation.AssignedRoomId?.Value,
            reservation.CheckInDate,
            reservation.CheckOutDate,
            reservation.GuestCount,
            reservation.TotalAmount.Amount,
            reservation.TotalAmount.Currency.ToString(),
            reservation.Status);
    }
}
