using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.Rooms.GetRoom;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Rooms.GetRoom;

[Collection(IntegrationTestCollection.Name)]
public class GetRoomHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public GetRoomHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_ExistingRoom_ReturnsRoomDetails()
    {
        var tenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        var room = Room.Create(tenantId, roomType.Id, RoomNumber.Create("A101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new GetRoomHandler(context);

            RoomDetails? details = await handler.HandleAsync(new GetRoomQuery(tenantId, room.Id));

            Assert.NotNull(details);
            Assert.Equal(room.Id, details.Id);
            Assert.Equal(roomType.Id, details.RoomTypeId);
            Assert.Equal("A101", details.Number);
            Assert.Equal(RoomCondition.Clean.ToString(), details.Condition);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoom_ReturnsNull()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        RoomType otherTenantRoomType = RoomTestData.CreateRoomType(otherTenantId);
        var room = Room.Create(otherTenantId, otherTenantRoomType.Id, RoomNumber.Create("A101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(otherTenantRoomType);
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new GetRoomHandler(context);

            RoomDetails? details = await handler.HandleAsync(new GetRoomQuery(tenantId, room.Id));

            Assert.Null(details);
        }
    }
}
