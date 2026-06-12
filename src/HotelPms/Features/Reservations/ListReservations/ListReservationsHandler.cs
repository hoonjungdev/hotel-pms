using HotelPms.Features.Reservations.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.ListReservations;

public class ListReservationsHandler(HotelDbContext context)
{
    public async Task<List<ReservationListItem>> HandleAsync(
        ListReservationsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Reservation>()
            .Where(reservation => reservation.TenantId == query.TenantId)
            .OrderBy(reservation => reservation.StayPeriod.Start)
            .ThenBy(reservation => reservation.Id)
            .AsNoTracking()
            .Select(reservation => new ReservationListItem(
                reservation.Id,
                reservation.PrimaryGuestId,
                reservation.RoomTypeId,
                reservation.AssignedRoomId,
                reservation.StayPeriod.Start,
                reservation.StayPeriod.End,
                reservation.GuestCount,
                reservation.TotalAmount,
                reservation.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}
