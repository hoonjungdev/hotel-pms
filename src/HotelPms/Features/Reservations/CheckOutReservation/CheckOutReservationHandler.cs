using FluentValidation;
using FluentValidation.Results;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.CheckOutReservation;

public class CheckOutReservationHandler(
    HotelDbContext context,
    IValidator<CheckOutReservationCommand> validator)
{
    public async Task<CheckOutReservationResult?> HandleAsync(
        CheckOutReservationCommand command,
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

        if (reservation.AssignedRoomId is null)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(CheckOutReservationCommand.ReservationId), "Reservation must have an assigned room before check-out.")
            ]);
        }

        Room room = await context.Set<Room>()
            .SingleOrDefaultAsync(
                candidate => candidate.TenantId == command.TenantId && candidate.Id == reservation.AssignedRoomId,
                cancellationToken) ?? throw new ValidationException(
            [
                new ValidationFailure(nameof(CheckOutReservationCommand.ReservationId), "Assigned room must exist for check-out.")
            ]);

        try
        {
            reservation.CheckOut(room);
        }
        catch (InvalidOperationException exception)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(CheckOutReservationCommand.ReservationId), exception.Message)
            ]);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CheckOutReservationResult(
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
