using FluentValidation;
using FluentValidation.Results;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.ConfirmReservation;

public class ConfirmReservationHandler(HotelDbContext context)
{
    public async Task<ConfirmReservationResult?> HandleAsync(
        ConfirmReservationCommand command,
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
            reservation.Confirm();
        }
        catch (InvalidOperationException exception)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(ConfirmReservationCommand.ReservationId), exception.Message)
            ]);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ConfirmReservationResult(
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
