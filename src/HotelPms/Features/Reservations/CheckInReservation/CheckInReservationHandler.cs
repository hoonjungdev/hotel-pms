using FluentValidation;
using FluentValidation.Results;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.CheckInReservation;

public class CheckInReservationHandler(
    HotelDbContext context,
    IValidator<CheckInReservationCommand> validator)
{
    public async Task<CheckInReservationResult?> HandleAsync(
        CheckInReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        Reservation? reservation = await context.Set<Reservation>()
            .SingleOrDefaultAsync(
                candidate => candidate.TenantId == command.TenantId && candidate.Id == command.ReservationId,
                cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        Room room = await context.Set<Room>()
            .SingleOrDefaultAsync(
                candidate => candidate.TenantId == command.TenantId && candidate.Id == command.RoomId,
                cancellationToken) ?? throw new ValidationException(
            [
                new ValidationFailure(nameof(CheckInReservationCommand.RoomId), "Room must exist for the tenant.")
            ]);

        bool roomAlreadyAssigned = await context.Set<Reservation>()
            .AnyAsync(
                candidate =>
                    candidate.TenantId == command.TenantId &&
                    candidate.Id != command.ReservationId &&
                    candidate.AssignedRoomId == command.RoomId &&
                    candidate.Status == ReservationStatus.CheckedIn &&
                    candidate.StayPeriod.Start < reservation.StayPeriod.End &&
                    reservation.StayPeriod.Start < candidate.StayPeriod.End,
                cancellationToken);

        if (roomAlreadyAssigned)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(CheckInReservationCommand.RoomId), "Room is already assigned for an overlapping stay.")
            ]);
        }

        try
        {
            reservation.CheckIn(room);
        }
        catch (InvalidOperationException exception)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(CheckInReservationCommand.ReservationId), exception.Message)
            ]);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CheckInReservationResult(
            reservation.Id,
            reservation.PrimaryGuestId,
            reservation.RoomTypeId,
            reservation.AssignedRoomId,
            reservation.StayPeriod.Start,
            reservation.StayPeriod.End,
            reservation.GuestCount,
            reservation.TotalAmount,
            reservation.Status.ToString());
    }
}
