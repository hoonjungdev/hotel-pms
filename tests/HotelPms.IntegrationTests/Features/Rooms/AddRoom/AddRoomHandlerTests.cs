using FluentValidation;
using HotelPms.Features.Rooms.AddRoom;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Rooms.AddRoom;

[Collection(IntegrationTestCollection.Name)]
public class AddRoomHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public AddRoomHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_PersistsRoom()
    {
        var tenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        var command = new AddRoomCommand(tenantId, roomType.Id, "  a101 ");

        AddRoomResult result;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();

            var handler = new AddRoomHandler(context, new AddRoomCommandValidator());

            result = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Room room = await context.Set<Room>().SingleAsync(candidate => candidate.Id == result.Id);

            Assert.Equal(tenantId, room.TenantId);
            Assert.Equal(roomType.Id, room.RoomTypeId);
            Assert.Equal("A101", room.Number.Value);
            Assert.Equal(RoomCondition.Clean, room.Condition);
        }

        Assert.Equal(roomType.Id, result.RoomTypeId);
        Assert.Equal("A101", result.Number);
        Assert.Equal(RoomCondition.Clean.ToString(), result.Condition);
    }

    [Fact]
    public async Task HandleAsync_MissingRoomType_ThrowsValidationException()
    {
        var command = new AddRoomCommand(TenantId.New(), RoomTypeId.New(), "A101");

        await using HotelDbContext context = _fixture.CreateDbContext();
        var handler = new AddRoomHandler(context, new AddRoomCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_MissingNumber_ThrowsValidationException()
    {
        var command = new AddRoomCommand(TenantId.New(), RoomTypeId.New(), "");

        await using HotelDbContext context = _fixture.CreateDbContext();
        var handler = new AddRoomHandler(context, new AddRoomCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }
}
