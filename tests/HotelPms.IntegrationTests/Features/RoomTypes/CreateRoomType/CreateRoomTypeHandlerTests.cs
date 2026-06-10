using FluentValidation;
using HotelPms.Features.RoomTypes.CreateRoomType;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.RoomTypes.CreateRoomType;

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

        CreateRoomTypeResult result;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new CreateRoomTypeHandler(context, new CreateRoomTypeCommandValidator());

            result = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            RoomType roomType = await context.Set<RoomType>().SingleAsync(candidate => candidate.Id == result.Id);

            Assert.Equal(tenantId, roomType.TenantId);
            Assert.Equal("DBL", roomType.Code.Value);
            Assert.Equal("Double", roomType.Name);
            Assert.Equal(2, roomType.BaseOccupancy);
            Assert.Equal(4, roomType.MaxOccupancy);
        }

        Assert.Equal("DBL", result.Code);
        Assert.Equal("Double", result.Name);
        Assert.Equal(2, result.BaseOccupancy);
        Assert.Equal(4, result.MaxOccupancy);
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

        CreateRoomTypeResult firstResult = await handler.HandleAsync(first);
        CreateRoomTypeResult secondResult = await handler.HandleAsync(second);

        int savedCount = await context.Set<RoomType>()
            .CountAsync(candidate => candidate.Id == firstResult.Id || candidate.Id == secondResult.Id);

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
