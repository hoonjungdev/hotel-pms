using FluentValidation;
using HotelPms.Features.Rooms.Application;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Rooms;

[Collection(IntegrationTestCollection.Name)]
public class CreateRoomTypeHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public CreateRoomTypeHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_PersistsRoomType()
    {
        var tenantId = TenantId.New();
        var command = new CreateRoomTypeCommand(tenantId, " dbl ", "  Double  ", 2, 4);

        RoomTypeId roomTypeId;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new CreateRoomTypeHandler(context, new CreateRoomTypeCommandValidator());

            roomTypeId = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            RoomType roomType = await context.Set<RoomType>().SingleAsync(candidate => candidate.Id == roomTypeId);

            Assert.Equal(tenantId, roomType.TenantId);
            Assert.Equal("DBL", roomType.Code.Value);
            Assert.Equal("Double", roomType.Name);
            Assert.Equal(2, roomType.BaseOccupancy);
            Assert.Equal(4, roomType.MaxOccupancy);
        }
    }

    [Fact]
    public async Task HandleAsync_DuplicateTenantCode_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        var first = new CreateRoomTypeCommand(tenantId, "dbl", "Double", 2, 4);
        var second = new CreateRoomTypeCommand(tenantId, " DBL ", "Deluxe Double", 2, 4);

        await using HotelDbContext context = _fixture.CreateDbContext();
        var handler = new CreateRoomTypeHandler(context, new CreateRoomTypeCommandValidator());

        await handler.HandleAsync(first);

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(second));
    }

    [Fact]
    public async Task HandleAsync_SameCodeDifferentTenants_PersistsRoomTypes()
    {
        var first = new CreateRoomTypeCommand(TenantId.New(), "dbl", "Double", 2, 4);
        var second = new CreateRoomTypeCommand(TenantId.New(), "DBL", "Double", 2, 4);

        await using HotelDbContext context = _fixture.CreateDbContext();
        var handler = new CreateRoomTypeHandler(context, new CreateRoomTypeCommandValidator());

        RoomTypeId firstId = await handler.HandleAsync(first);
        RoomTypeId secondId = await handler.HandleAsync(second);

        int savedCount = await context.Set<RoomType>()
            .CountAsync(candidate => candidate.Id == firstId || candidate.Id == secondId);

        Assert.Equal(2, savedCount);
    }

    [Fact]
    public async Task HandleAsync_MaxOccupancyBelowBaseOccupancy_ThrowsValidationException()
    {
        var command = new CreateRoomTypeCommand(TenantId.New(), "dbl", "Double", 3, 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        var handler = new CreateRoomTypeHandler(context, new CreateRoomTypeCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }
}
