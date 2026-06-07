using HotelPms.Features.Rooms.Application;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Rooms;

[Collection(IntegrationTestCollection.Name)]
public class UpdateRoomConditionHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public UpdateRoomConditionHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_CleanRoomToDirty_PersistsDirtyCondition()
    {
        var tenantId = TenantId.New();
        var room = Room.Create(tenantId, RoomNumber.Create("a101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new UpdateRoomConditionHandler(context, new UpdateRoomConditionCommandValidator());
            await handler.HandleAsync(new UpdateRoomConditionCommand(tenantId, room.Id, RoomCondition.Dirty));
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Room restored = await context.Set<Room>().SingleAsync(candidate => candidate.Id == room.Id);

            Assert.Equal(RoomCondition.Dirty, restored.Condition);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoom_DoesNotUpdateRoom()
    {
        var tenantId1 = TenantId.New();
        var tenantId2 = TenantId.New();

        var room = Room.Create(tenantId1, RoomNumber.Create("a101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new UpdateRoomConditionHandler(context, new UpdateRoomConditionCommandValidator());
            await handler.HandleAsync(new UpdateRoomConditionCommand(tenantId2, room.Id, RoomCondition.Dirty));
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Room restored = await context.Set<Room>().SingleAsync(candidate => candidate.Id == room.Id);

            Assert.Equal(RoomCondition.Clean, restored.Condition);
        }
    }

    [Fact]
    public async Task HandleAsync_MissingRoom_DoesNotThrow()
    {
        var command = new UpdateRoomConditionCommand(
            TenantId.New(),
            RoomId.New(),
            RoomCondition.Dirty);

        await using HotelDbContext context = _fixture.CreateDbContext();
        var handler = new UpdateRoomConditionHandler(context, new UpdateRoomConditionCommandValidator());

        await handler.HandleAsync(command);
    }
}
