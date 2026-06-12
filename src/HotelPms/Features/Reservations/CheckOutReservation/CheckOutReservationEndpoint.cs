using FluentValidation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Reservations.CheckOutReservation;

internal static class CheckOutReservationEndpoint
{
    public static RouteGroupBuilder MapCheckOutReservationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/{reservationId:guid}/check-out", CheckOutReservationAsync)
            .WithName("CheckOutReservation");

        return group;
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound, ValidationProblem>> CheckOutReservationAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid reservationId,
        CheckOutReservationHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            CheckOutReservationResult? result = await handler.HandleAsync(
                new CheckOutReservationCommand(
                    new TenantId(tenantId),
                    new ReservationId(reservationId)),
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

    private static ReservationResponse ToResponse(CheckOutReservationResult result)
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
