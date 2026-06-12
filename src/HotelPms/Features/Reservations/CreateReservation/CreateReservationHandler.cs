using FluentValidation;
using FluentValidation.Results;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Pricing.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Reservations.Infrastructure;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HotelPms.Features.Reservations.CreateReservation;

public class CreateReservationHandler(HotelDbContext context, IValidator<CreateReservationCommand> validator)
{
    public async Task<CreateReservationResult> HandleAsync(
        CreateReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        bool guestExists = await context.Set<Guest>()
            .AnyAsync(
                guest => guest.TenantId == command.TenantId && guest.Id == command.PrimaryGuestId,
                cancellationToken);

        if (!guestExists)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(CreateReservationCommand.PrimaryGuestId),
                    "Primary guest must exist for the tenant.")
            ]);
        }

        RoomType? roomType = await context.Set<RoomType>()
            .SingleOrDefaultAsync(
                candidate => candidate.TenantId == command.TenantId && candidate.Id == command.RoomTypeId,
                cancellationToken) ?? throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(CreateReservationCommand.RoomTypeId),
                    "Room type must exist for the tenant.")
            ]);
        if (command.GuestCount > roomType.MaxOccupancy)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(CreateReservationCommand.GuestCount),
                    "Guest count cannot exceed the room type max occupancy.")
            ]);
        }

        var stayPeriod = new DateRange(command.CheckInDate, command.CheckOutDate);

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.AcquireReservationCreationLockAsync(command.TenantId, command.RoomTypeId, cancellationToken);

        ReservationAvailability availability = await context.GetReservationAvailabilityAsync(
            command.TenantId,
            command.RoomTypeId,
            stayPeriod,
            cancellationToken);

        if (!availability.HasAvailability)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(CreateReservationCommand.RoomTypeId),
                    "No rooms are available for the requested stay period.")
            ]);
        }

        var reservation = Reservation.Create(
            command.TenantId,
            command.PrimaryGuestId,
            command.RoomTypeId,
            stayPeriod,
            command.GuestCount,
            PriceCalculator.CalculateStayTotal(roomType.BaseNightlyRate, stayPeriod));

        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CreateReservationResult(
            reservation.Id,
            reservation.PrimaryGuestId,
            reservation.RoomTypeId,
            reservation.StayPeriod.Start,
            reservation.StayPeriod.End,
            reservation.GuestCount,
            reservation.TotalAmount,
            reservation.Status.ToString());
    }
}
