using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HotelPms.Features.Reservations.Infrastructure;

public static class ReservationAvailabilityOperations
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

    public static async Task<Reservation> AddReservationWhenAvailableAsync(
        this HotelDbContext context,
        TenantId tenantId,
        RoomTypeId roomTypeId,
        DateRange stayPeriod,
        Func<Reservation> createReservation,
        CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.AcquireReservationCreationLockAsync(tenantId, roomTypeId, cancellationToken);

        ReservationAvailability availability = await context.GetReservationAvailabilityAsync(
            tenantId,
            roomTypeId,
            stayPeriod,
            cancellationToken);

        if (!availability.HasAvailability)
        {
            throw new NoReservationAvailabilityException(roomTypeId, stayPeriod);
        }

        Reservation reservation = createReservation();
        context.Set<Reservation>().Add(reservation);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return reservation;
    }

    private static async Task AcquireReservationCreationLockAsync(
        this HotelDbContext context,
        TenantId tenantId,
        RoomTypeId roomTypeId,
        CancellationToken cancellationToken)
    {
        string lockKey = $"{tenantId.Value:N}:{roomTypeId.Value:N}";

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
            cancellationToken);
    }
}
