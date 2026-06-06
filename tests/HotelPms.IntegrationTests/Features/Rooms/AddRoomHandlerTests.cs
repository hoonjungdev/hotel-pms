using FluentValidation;
using HotelPms.Features.Rooms.Application;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Rooms;

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
        var command = new AddRoomCommand(tenantId, "  a101 ");

        RoomId roomId;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new AddRoomHandler(context, new AddRoomCommandValidator());

            roomId = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Room room = await context.Set<Room>().SingleAsync(candidate => candidate.Id == roomId);

            Assert.Equal(tenantId, room.TenantId);
            Assert.Equal("A101", room.Number.Value);
            Assert.Equal(RoomCondition.Clean, room.Condition);
        }
    }

    [Fact]
    public async Task HandleAsync_MissingNumber_ThrowsValidationException()
    {
        var command = new AddRoomCommand(TenantId.New(), "");

        await using HotelDbContext context = _fixture.CreateDbContext();
        var handler = new AddRoomHandler(context, new AddRoomCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }
}
