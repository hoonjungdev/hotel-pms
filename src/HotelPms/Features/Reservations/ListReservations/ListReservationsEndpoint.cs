using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Reservations.ListReservations;

internal static class ListReservationsEndpoint
{
    public static RouteGroupBuilder MapListReservationsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListReservationsAsync)
            .WithName("ListReservations");

        return group;
    }

    private static async Task<Ok<IReadOnlyList<ReservationResponse>>> ListReservationsAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        ListReservationsHandler handler,
        CancellationToken cancellationToken)
    {
        List<ReservationListItem> reservations = await handler.HandleAsync(
            new ListReservationsQuery(new TenantId(tenantId)),
            cancellationToken);

        IReadOnlyList<ReservationResponse> response = reservations
            .Select(ToResponse)
            .ToList();

        return TypedResults.Ok(response);
    }

    private static ReservationResponse ToResponse(ReservationListItem reservation)
    {
        return new ReservationResponse(
            reservation.Id.Value,
            reservation.PrimaryGuestId.Value,
            reservation.RoomTypeId.Value,
            reservation.CheckInDate,
            reservation.CheckOutDate,
            reservation.GuestCount,
            reservation.Status);
    }
}
