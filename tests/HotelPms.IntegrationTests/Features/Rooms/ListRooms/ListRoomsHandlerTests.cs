using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.Rooms.ListRooms;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Rooms.ListRooms;

[Collection(IntegrationTestCollection.Name)]
public class ListRoomsHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public ListRoomsHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_SameTenantRooms_ReturnsRoomsOrderedByNumber()
    {
        var tenantId = TenantId.New();

        var room1 = Room.Create(tenantId, RoomNumber.Create("a102"));
        var room2 = Room.Create(tenantId, RoomNumber.Create("a101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Room>().AddRange(room1, room2);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListRoomsHandler(context);
            List<RoomListItem> rooms = await handler.HandleAsync(new ListRoomsQuery(tenantId));

            Assert.Equal(2, rooms.Count);
            Assert.Equal("A101", rooms[0].Number);
            Assert.Equal("A102", rooms[1].Number);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoom_IsExcludedFromResults()
    {
        var tenantId1 = TenantId.New();
        var tenantId2 = TenantId.New();

        var room1 = Room.Create(tenantId1, RoomNumber.Create("a101"));
        var room2 = Room.Create(tenantId2, RoomNumber.Create("a102"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Room>().AddRange(room1, room2);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListRoomsHandler(context);
            List<RoomListItem> rooms = await handler.HandleAsync(new ListRoomsQuery(tenantId1));

            Assert.Single(rooms);
            Assert.Equal("A101", rooms[0].Number);
        }
    }

    [Fact]
    public async Task HandleAsync_NoRooms_ReturnsEmptyList()
    {
        var tenantId = TenantId.New();

        await using HotelDbContext context = _fixture.CreateDbContext();

        var handler = new ListRoomsHandler(context);
        List<RoomListItem> rooms = await handler.HandleAsync(new ListRoomsQuery(tenantId));

        Assert.Empty(rooms);
    }
}
