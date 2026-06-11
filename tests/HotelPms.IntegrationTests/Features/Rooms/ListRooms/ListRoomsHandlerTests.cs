using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.Rooms.ListRooms;
using HotelPms.Features.RoomTypes.Domain;
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
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);

        var room1 = Room.Create(tenantId, roomType.Id, RoomNumber.Create("a102"));
        var room2 = Room.Create(tenantId, roomType.Id, RoomNumber.Create("a101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().AddRange(room1, room2);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListRoomsHandler(context);
            List<RoomListItem> rooms = await handler.HandleAsync(new ListRoomsQuery(tenantId));

            Assert.Equal(2, rooms.Count);
            Assert.Equal("A101", rooms[0].Number);
            Assert.Equal(roomType.Id, rooms[0].RoomTypeId);
            Assert.Equal("A102", rooms[1].Number);
            Assert.Equal(roomType.Id, rooms[1].RoomTypeId);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoom_IsExcludedFromResults()
    {
        var tenantId1 = TenantId.New();
        var tenantId2 = TenantId.New();
        RoomType roomType1 = RoomTestData.CreateRoomType(tenantId1);
        RoomType roomType2 = RoomTestData.CreateRoomType(tenantId2);

        var room1 = Room.Create(tenantId1, roomType1.Id, RoomNumber.Create("a101"));
        var room2 = Room.Create(tenantId2, roomType2.Id, RoomNumber.Create("a102"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().AddRange(roomType1, roomType2);
            context.Set<Room>().AddRange(room1, room2);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListRoomsHandler(context);
            List<RoomListItem> rooms = await handler.HandleAsync(new ListRoomsQuery(tenantId1));

            Assert.Single(rooms);
            Assert.Equal("A101", rooms[0].Number);
            Assert.Equal(roomType1.Id, rooms[0].RoomTypeId);
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
