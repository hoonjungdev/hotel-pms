using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.RoomTypes;

[Collection(IntegrationTestCollection.Name)]
public class RoomTypePersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public RoomTypePersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Save_ValidRoomType_RestoresRoomType()
    {
        var roomType = RoomType.Create(
            TenantId.New(),
            RoomTypeCode.Create(" dbl "),
            "  Double  ",
            2,
            4,
            new Money(120_000, Currency.KRW));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            RoomType restored = await context.Set<RoomType>().SingleAsync(candidate => candidate.Id == roomType.Id);

            Assert.Equal(roomType.Id, restored.Id);
            Assert.Equal(roomType.TenantId, restored.TenantId);
            Assert.Equal("DBL", restored.Code.Value);
            Assert.Equal("Double", restored.Name);
            Assert.Equal(2, restored.BaseOccupancy);
            Assert.Equal(4, restored.MaxOccupancy);
            Assert.Equal(new Money(120_000, Currency.KRW), restored.BaseNightlyRate);
        }
    }

    [Fact]
    public async Task Save_DuplicateTenantCode_ThrowsDbUpdateException()
    {
        var tenantId = TenantId.New();
        var first = RoomType.Create(tenantId, RoomTypeCode.Create("dbl"), "Double", 2, 4, new Money(120_000, Currency.KRW));
        var second = RoomType.Create(tenantId, RoomTypeCode.Create("DBL"), "Deluxe Double", 2, 4, new Money(150_000, Currency.KRW));

        await using HotelDbContext context = _fixture.CreateDbContext();

        context.Set<RoomType>().AddRange(first, second);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Save_SameCodeDifferentTenants_PersistsRoomTypes()
    {
        var first = RoomType.Create(TenantId.New(), RoomTypeCode.Create("dbl"), "Double", 2, 4, new Money(120_000, Currency.KRW));
        var second = RoomType.Create(TenantId.New(), RoomTypeCode.Create("DBL"), "Double", 2, 4, new Money(120_000, Currency.KRW));

        await using HotelDbContext context = _fixture.CreateDbContext();

        context.Set<RoomType>().AddRange(first, second);
        await context.SaveChangesAsync();

        int savedCount = await context.Set<RoomType>()
            .CountAsync(candidate => candidate.Id == first.Id || candidate.Id == second.Id);

        Assert.Equal(2, savedCount);
    }
}
