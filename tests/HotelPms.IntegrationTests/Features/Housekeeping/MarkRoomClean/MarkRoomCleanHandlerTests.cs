using FluentValidation;
using HotelPms.Features.Housekeeping.MarkRoomClean;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Features.Rooms;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Housekeeping.MarkRoomClean;

[Collection(IntegrationTestCollection.Name)]
public class MarkRoomCleanHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public MarkRoomCleanHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_DirtyRoom_MarksRoomClean()
    {
        var tenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        Room room = RoomTestData.CreateRoom(tenantId, roomType, "A101");
        room.MarkDirty();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new MarkRoomCleanHandler(context, new MarkRoomCleanCommandValidator());

            MarkRoomCleanResult? result = await handler.HandleAsync(new MarkRoomCleanCommand(tenantId, room.Id));

            Assert.NotNull(result);
            Assert.Equal(room.Id, result.Id);
            Assert.Equal("A101", result.Number);
            Assert.Equal(RoomCondition.Clean.ToString(), result.Condition);
        }

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        Room restored = await restoredContext.Set<Room>().SingleAsync(candidate => candidate.Id == room.Id);

        Assert.Equal(RoomCondition.Clean, restored.Condition);
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoom_ReturnsNull()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(otherTenantId);
        Room room = RoomTestData.CreateRoom(otherTenantId, roomType, "A101");
        room.MarkDirty();

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        await context.SaveChangesAsync();

        var handler = new MarkRoomCleanHandler(context, new MarkRoomCleanCommandValidator());

        MarkRoomCleanResult? result = await handler.HandleAsync(new MarkRoomCleanCommand(tenantId, room.Id));

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_OutOfServiceRoom_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        Room room = RoomTestData.CreateRoom(tenantId, roomType, "A101");
        room.TakeOutOfService();

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        await context.SaveChangesAsync();

        var handler = new MarkRoomCleanHandler(context, new MarkRoomCleanCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(new MarkRoomCleanCommand(tenantId, room.Id)));
    }
}
