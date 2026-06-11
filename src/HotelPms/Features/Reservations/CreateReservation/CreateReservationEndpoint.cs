using FluentValidation;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Reservations.CreateReservation;

internal static class CreateReservationEndpoint
{
    public static RouteGroupBuilder MapCreateReservationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateReservationAsync)
            .WithName("CreateReservation");

        return group;
    }

    private static async Task<Results<Created<ReservationResponse>, ValidationProblem>> CreateReservationAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        CreateReservationRequest request,
        CreateReservationHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateReservationCommand(
            new TenantId(tenantId),
            new GuestId(request.PrimaryGuestId),
            new RoomTypeId(request.RoomTypeId),
            request.CheckInDate,
            request.CheckOutDate,
            request.GuestCount);

        try
        {
            CreateReservationResult result = await handler.HandleAsync(command, cancellationToken);

            return TypedResults.Created($"/api/reservations/{result.Id.Value}", ToResponse(result));
        }
        catch (ValidationException exception)
        {
            return TypedResults.ValidationProblem(ValidationProblemMapper.ToErrors(exception, "Reservation"));
        }
    }

    private static ReservationResponse ToResponse(CreateReservationResult result)
    {
        return new ReservationResponse(
            result.Id.Value,
            result.PrimaryGuestId.Value,
            result.RoomTypeId.Value,
            result.CheckInDate,
            result.CheckOutDate,
            result.GuestCount,
            result.Status);
    }
}
