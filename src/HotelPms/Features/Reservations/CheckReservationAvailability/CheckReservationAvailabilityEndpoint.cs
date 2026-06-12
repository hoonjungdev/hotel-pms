using FluentValidation;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Reservations.CheckReservationAvailability;

internal static class CheckReservationAvailabilityEndpoint
{
    public static RouteGroupBuilder MapCheckReservationAvailabilityEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/availability", CheckReservationAvailabilityAsync)
            .WithName("CheckReservationAvailability");

        return group;
    }

    private static async Task<Results<Ok<CheckReservationAvailabilityResponse>, ValidationProblem>>
        CheckReservationAvailabilityAsync(
            [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
            [FromQuery] Guid roomTypeId,
            [FromQuery] DateOnly checkInDate,
            [FromQuery] DateOnly checkOutDate,
            CheckReservationAvailabilityHandler handler,
            CancellationToken cancellationToken)
    {
        try
        {
            ReservationAvailabilityDetails availability = await handler.HandleAsync(
                new CheckReservationAvailabilityQuery(
                    new TenantId(tenantId),
                    new RoomTypeId(roomTypeId),
                    checkInDate,
                    checkOutDate),
                cancellationToken);

            return TypedResults.Ok(ToResponse(availability));
        }
        catch (ValidationException exception)
        {
            return TypedResults.ValidationProblem(ValidationProblemMapper.ToErrors(exception, "Availability"));
        }
    }

    private static CheckReservationAvailabilityResponse ToResponse(ReservationAvailabilityDetails availability)
    {
        return new CheckReservationAvailabilityResponse(
            availability.RoomTypeId.Value,
            availability.CheckInDate,
            availability.CheckOutDate,
            availability.SellableRoomCount,
            availability.ActiveReservationCount,
            availability.AvailableRoomCount,
            availability.HasAvailability);
    }
}
