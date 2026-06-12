using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Reservations.Infrastructure;

public static class ReservationConcurrencyLocks
{
    public static async Task AcquireReservationCreationLockAsync(
        this HotelDbContext context,
        TenantId tenantId,
        RoomTypeId roomTypeId,
        CancellationToken cancellationToken = default)
    {
        string lockKey = $"{tenantId.Value:N}:{roomTypeId.Value:N}";

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
            cancellationToken);
    }
}
