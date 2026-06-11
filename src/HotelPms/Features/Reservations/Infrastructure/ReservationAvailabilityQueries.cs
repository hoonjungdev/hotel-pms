using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.Infrastructure;

public static class ReservationAvailabilityQueries
{
    public static async Task<ReservationAvailability> GetReservationAvailabilityAsync(
        this HotelDbContext context,
        TenantId tenantId,
        RoomTypeId roomTypeId,
        DateRange stayPeriod,
        CancellationToken cancellationToken = default)
    {
        int sellableRoomCount = await context.Set<Room>()
            .CountAsync(
                room =>
                    room.TenantId == tenantId &&
                    room.RoomTypeId == roomTypeId &&
                    room.Condition != RoomCondition.OutOfService,
                cancellationToken);

        int activeReservationCount = await context.Set<Reservation>()
            .CountAsync(
                reservation =>
                    reservation.TenantId == tenantId &&
                    reservation.RoomTypeId == roomTypeId &&
                    reservation.Status != ReservationStatus.Cancelled &&
                    reservation.StayPeriod.Start < stayPeriod.End &&
                    stayPeriod.Start < reservation.StayPeriod.End,
                cancellationToken);

        return new ReservationAvailability(
            roomTypeId,
            stayPeriod,
            sellableRoomCount,
            activeReservationCount);
    }
}
