using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.Rooms.GetRoom;
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
        var room = Room.Create(tenantId, RoomNumber.Create("A101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new GetRoomHandler(context);

            RoomDetails? details = await handler.HandleAsync(new GetRoomQuery(tenantId, room.Id));

            Assert.NotNull(details);
            Assert.Equal(room.Id, details.Id);
            Assert.Equal("A101", details.Number);
            Assert.Equal(RoomCondition.Clean.ToString(), details.Condition);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoom_ReturnsNull()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        var room = Room.Create(otherTenantId, RoomNumber.Create("A101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
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
