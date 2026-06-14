using HotelPms.Features.Housekeeping.ListHousekeepingRooms;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Features.Rooms;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Housekeeping.ListHousekeepingRooms;

[Collection(IntegrationTestCollection.Name)]
public class ListHousekeepingRoomsHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public ListHousekeepingRoomsHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_SameTenantRooms_ReturnsRoomsOrderedByHousekeepingPriority()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        RoomType otherTenantRoomType = RoomTestData.CreateRoomType(otherTenantId);

        Room cleanRoom = RoomTestData.CreateRoom(tenantId, roomType, "A103");
        Room dirtyRoom = RoomTestData.CreateRoom(tenantId, roomType, "A101");
        Room outOfServiceRoom = RoomTestData.CreateRoom(tenantId, roomType, "A102");
        Room excludedRoom = RoomTestData.CreateRoom(otherTenantId, otherTenantRoomType, "A100");

        dirtyRoom.MarkDirty();
        outOfServiceRoom.TakeOutOfService();
        excludedRoom.MarkDirty();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().AddRange(roomType, otherTenantRoomType);
            context.Set<Room>().AddRange(cleanRoom, dirtyRoom, outOfServiceRoom, excludedRoom);
            await context.SaveChangesAsync();
        }

        await using HotelDbContext queryContext = _fixture.CreateDbContext();
        var handler = new ListHousekeepingRoomsHandler(queryContext);

        List<HousekeepingRoomListItem> result = await handler.HandleAsync(new ListHousekeepingRoomsQuery(tenantId, null));

        Assert.Equal(3, result.Count);
        Assert.Equal(dirtyRoom.Id, result[0].Id);
        Assert.Equal(RoomCondition.Dirty.ToString(), result[0].Condition);
        Assert.Equal(outOfServiceRoom.Id, result[1].Id);
        Assert.Equal(RoomCondition.OutOfService.ToString(), result[1].Condition);
        Assert.Equal(cleanRoom.Id, result[2].Id);
        Assert.Equal(RoomCondition.Clean.ToString(), result[2].Condition);
    }

    [Fact]
    public async Task HandleAsync_DirtyCondition_ReturnsOnlyDirtyRooms()
    {
        var tenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        Room cleanRoom = RoomTestData.CreateRoom(tenantId, roomType, "A101");
        Room dirtyRoom = RoomTestData.CreateRoom(tenantId, roomType, "A102");

        dirtyRoom.MarkDirty();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().AddRange(cleanRoom, dirtyRoom);
            await context.SaveChangesAsync();
        }

        await using HotelDbContext queryContext = _fixture.CreateDbContext();
        var handler = new ListHousekeepingRoomsHandler(queryContext);

        List<HousekeepingRoomListItem> result = await handler.HandleAsync(
            new ListHousekeepingRoomsQuery(tenantId, RoomCondition.Dirty));

        HousekeepingRoomListItem room = Assert.Single(result);
        Assert.Equal(dirtyRoom.Id, room.Id);
        Assert.Equal(RoomCondition.Dirty.ToString(), room.Condition);
    }
}
