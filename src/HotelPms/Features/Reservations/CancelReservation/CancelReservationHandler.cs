using FluentValidation;
using FluentValidation.Results;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.CancelReservation;

public class CancelReservationHandler(HotelDbContext context)
{
    public async Task<CancelReservationResult?> HandleAsync(
        CancelReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        Reservation? reservation = await context.Set<Reservation>()
            .SingleOrDefaultAsync(
                candidate => candidate.TenantId == command.TenantId && candidate.Id == command.ReservationId,
                cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        try
        {
            reservation.Cancel();
        }
        catch (InvalidOperationException exception)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(CancelReservationCommand.ReservationId), exception.Message)
            ]);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CancelReservationResult(
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
