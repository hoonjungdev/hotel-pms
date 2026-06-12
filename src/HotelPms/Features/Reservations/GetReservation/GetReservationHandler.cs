using HotelPms.Features.Reservations.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.GetReservation;

public class GetReservationHandler(HotelDbContext context)
{
    public async Task<ReservationDetails?> HandleAsync(
        GetReservationQuery query,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Reservation>()
            .Where(reservation => reservation.TenantId == query.TenantId && reservation.Id == query.ReservationId)
            .AsNoTracking()
            .Select(reservation => new ReservationDetails(
                reservation.Id,
                reservation.PrimaryGuestId,
                reservation.RoomTypeId,
                reservation.StayPeriod.Start,
                reservation.StayPeriod.End,
                reservation.GuestCount,
                reservation.TotalAmount,
                reservation.Status.ToString()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
