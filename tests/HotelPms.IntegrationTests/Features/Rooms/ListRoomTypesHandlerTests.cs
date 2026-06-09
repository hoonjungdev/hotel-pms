using HotelPms.Features.Rooms.Application;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Rooms;

[Collection(IntegrationTestCollection.Name)]
public class ListRoomTypesHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public ListRoomTypesHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_SameTenantRoomTypes_ReturnsRoomTypesOrderedByCode()
    {
        var tenantId = TenantId.New();

        var deluxe = RoomType.Create(tenantId, RoomTypeCode.Create("DLX"), "Deluxe", 2, 4);
        var doubleRoom = RoomType.Create(tenantId, RoomTypeCode.Create("DBL"), "Double", 2, 2);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().AddRange(deluxe, doubleRoom);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListRoomTypesHandler(context);
            List<RoomTypeListItem> roomTypes = await handler.HandleAsync(new ListRoomTypesQuery(tenantId));

            Assert.Equal(2, roomTypes.Count);
            Assert.Equal("DBL", roomTypes[0].Code);
            Assert.Equal("DLX", roomTypes[1].Code);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoomType_IsExcludedFromResults()
    {
        var tenantId1 = TenantId.New();
        var tenantId2 = TenantId.New();

        var first = RoomType.Create(tenantId1, RoomTypeCode.Create("DBL"), "Double", 2, 2);
        var second = RoomType.Create(tenantId2, RoomTypeCode.Create("DLX"), "Deluxe", 2, 4);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().AddRange(first, second);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListRoomTypesHandler(context);
            List<RoomTypeListItem> roomTypes = await handler.HandleAsync(new ListRoomTypesQuery(tenantId1));

            Assert.Single(roomTypes);
            Assert.Equal("DBL", roomTypes[0].Code);
        }
    }

    [Fact]
    public async Task HandleAsync_NoRoomTypes_ReturnsEmptyList()
    {
        var tenantId = TenantId.New();

        await using HotelDbContext context = _fixture.CreateDbContext();

        var handler = new ListRoomTypesHandler(context);
        List<RoomTypeListItem> roomTypes = await handler.HandleAsync(new ListRoomTypesQuery(tenantId));

        Assert.Empty(roomTypes);
    }
}
