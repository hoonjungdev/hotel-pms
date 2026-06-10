using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.GetRoomType;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.RoomTypes.GetRoomType;

[Collection(IntegrationTestCollection.Name)]
public class GetRoomTypeHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public GetRoomTypeHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_ExistingRoomType_ReturnsRoomTypeDetails()
    {
        var tenantId = TenantId.New();
        var roomType = RoomType.Create(tenantId, RoomTypeCode.Create("DBL"), "Double", 2, 4);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new GetRoomTypeHandler(context);

            RoomTypeDetails? details = await handler.HandleAsync(new GetRoomTypeQuery(tenantId, roomType.Id));

            Assert.NotNull(details);
            Assert.Equal(roomType.Id, details.Id);
            Assert.Equal("DBL", details.Code);
            Assert.Equal("Double", details.Name);
            Assert.Equal(2, details.BaseOccupancy);
            Assert.Equal(4, details.MaxOccupancy);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoomType_ReturnsNull()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        var roomType = RoomType.Create(otherTenantId, RoomTypeCode.Create("DBL"), "Double", 2, 4);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new GetRoomTypeHandler(context);

            RoomTypeDetails? details = await handler.HandleAsync(new GetRoomTypeQuery(tenantId, roomType.Id));

            Assert.Null(details);
        }
    }

    [Fact]
    public async Task HandleAsync_MissingRoomType_ReturnsNull()
    {
        var tenantId = TenantId.New();

        await using HotelDbContext context = _fixture.CreateDbContext();
        var handler = new GetRoomTypeHandler(context);

        RoomTypeDetails? details = await handler.HandleAsync(new GetRoomTypeQuery(tenantId, RoomTypeId.New()));

        Assert.Null(details);
    }
}
