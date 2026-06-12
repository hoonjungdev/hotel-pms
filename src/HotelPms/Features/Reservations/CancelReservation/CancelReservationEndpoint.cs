using FluentValidation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Reservations.CancelReservation;

internal static class CancelReservationEndpoint
{
    public static RouteGroupBuilder MapCancelReservationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/{reservationId:guid}/cancel", CancelReservationAsync)
            .WithName("CancelReservation");

        return group;
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound, ValidationProblem>> CancelReservationAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid reservationId,
        CancelReservationHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            CancelReservationResult? result = await handler.HandleAsync(
                new CancelReservationCommand(new TenantId(tenantId), new ReservationId(reservationId)),
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

    private static ReservationResponse ToResponse(CancelReservationResult result)
    {
        return new ReservationResponse(
            result.Id.Value,
            result.PrimaryGuestId.Value,
            result.RoomTypeId.Value,
            result.CheckInDate,
            result.CheckOutDate,
            result.GuestCount,
            result.TotalAmount.Amount,
            result.TotalAmount.Currency.ToString(),
            result.Status);
    }
}
