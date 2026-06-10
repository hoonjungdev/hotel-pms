using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.Infrastructure.Database.Seed;
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

        Assert.Equal(3, roomTypeCount);
        Assert.Equal(4, roomCount);
        Assert.Equal(2, guestCount);
    }
}
