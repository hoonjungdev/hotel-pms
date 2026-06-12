using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.Infrastructure.Database.Seed;
using HotelPms.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public class DemoSeedDataTests
{
    private readonly PostgreSqlFixture _fixture;

    public DemoSeedDataTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_InsertsDemoDataOnce()
    {
        await using HotelDbContext context = _fixture.CreateDbContext();

        await DemoSeedData.SeedAsync(context);
        await DemoSeedData.SeedAsync(context);

        int roomTypeCount = await context.Set<RoomType>()
            .CountAsync(roomType => roomType.TenantId == DemoSeedData.TenantId);
        int roomCount = await context.Set<Room>()
            .CountAsync(room => room.TenantId == DemoSeedData.TenantId);
        int guestCount = await context.Set<Guest>()
            .CountAsync(guest => guest.TenantId == DemoSeedData.TenantId);
        List<Reservation> reservations = await context.Set<Reservation>()
            .Where(reservation => reservation.TenantId == DemoSeedData.TenantId)
            .ToListAsync();

        Assert.Equal(3, roomTypeCount);
        Assert.Equal(4, roomCount);
        Assert.Equal(2, guestCount);
        Reservation reservation = Assert.Single(reservations);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(new DateOnly(2026, 7, 10), reservation.StayPeriod.Start);
        Assert.Equal(new DateOnly(2026, 7, 12), reservation.StayPeriod.End);
        Assert.Equal(new Money(160_000, Currency.KRW), reservation.TotalAmount);
    }
}
