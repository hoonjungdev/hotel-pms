using FluentValidation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Reservations.ConfirmReservation;

internal static class ConfirmReservationEndpoint
{
    public static RouteGroupBuilder MapConfirmReservationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/{reservationId:guid}/confirm", ConfirmReservationAsync)
            .WithName("ConfirmReservation");

        return group;
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound, ValidationProblem>> ConfirmReservationAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid reservationId,
        ConfirmReservationHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            ConfirmReservationResult? result = await handler.HandleAsync(
                new ConfirmReservationCommand(new TenantId(tenantId), new ReservationId(reservationId)),
                cancellationToken);

            return result is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(ToResponse(result));
        }
        catch (ValidationException exception)
        {
            return TypedResults.ValidationProblem(ValidationProblemMapper.ToErrors(exception, "Reservation"));
        }
    }

    private static ReservationResponse ToResponse(ConfirmReservationResult result)
    {
        return new ReservationResponse(
            result.Id.Value,
            result.PrimaryGuestId.Value,
            result.RoomTypeId.Value,
            result.AssignedRoomId?.Value,
            result.CheckInDate,
            result.CheckOutDate,
            result.GuestCount,
            result.TotalAmount.Amount,
            result.TotalAmount.Currency.ToString(),
            result.Status);
    }
}
