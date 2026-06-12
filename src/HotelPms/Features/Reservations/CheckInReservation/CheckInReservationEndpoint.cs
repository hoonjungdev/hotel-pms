using FluentValidation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Reservations.CheckInReservation;

internal static class CheckInReservationEndpoint
{
    public static RouteGroupBuilder MapCheckInReservationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/{reservationId:guid}/check-in", CheckInReservationAsync)
            .WithName("CheckInReservation");

        return group;
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound, ValidationProblem>> CheckInReservationAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid reservationId,
        CheckInReservationRequest request,
        CheckInReservationHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            CheckInReservationResult? result = await handler.HandleAsync(
                new CheckInReservationCommand(
                    new TenantId(tenantId),
                    new ReservationId(reservationId),
                    new RoomId(request.RoomId)),
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

    private static ReservationResponse ToResponse(CheckInReservationResult result)
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
