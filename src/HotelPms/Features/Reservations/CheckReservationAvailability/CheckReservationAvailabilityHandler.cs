using FluentValidation;
using FluentValidation.Results;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Reservations.Infrastructure;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.CheckReservationAvailability;

public class CheckReservationAvailabilityHandler(
    HotelDbContext context,
    IValidator<CheckReservationAvailabilityQuery> validator)
{
    public async Task<ReservationAvailabilityDetails> HandleAsync(
        CheckReservationAvailabilityQuery query,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        bool roomTypeExists = await context.Set<RoomType>()
            .AnyAsync(
                roomType => roomType.TenantId == query.TenantId && roomType.Id == query.RoomTypeId,
                cancellationToken);

        if (!roomTypeExists)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(CheckReservationAvailabilityQuery.RoomTypeId),
                    "Room type must exist for the tenant.")
            ]);
        }

        var stayPeriod = new DateRange(query.CheckInDate, query.CheckOutDate);
        ReservationAvailability availability = await context.GetReservationAvailabilityAsync(
            query.TenantId,
            query.RoomTypeId,
            stayPeriod,
            cancellationToken);

        return new ReservationAvailabilityDetails(
            availability.RoomTypeId,
            availability.StayPeriod.Start,
            availability.StayPeriod.End,
            availability.SellableRoomCount,
            availability.ActiveReservationCount,
            availability.AvailableRoomCount,
            availability.HasAvailability);
    }
}
