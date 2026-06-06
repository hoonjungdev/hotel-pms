using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Rooms;

[Collection(IntegrationTestCollection.Name)]
public class RoomPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public RoomPersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Save_CleanRoom_RestoresRoom()
    {
        var room = Room.Create(TenantId.New(), RoomNumber.Create(" a101  "));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Room restored = await context.Set<Room>().SingleAsync(candidate => candidate.TenantId == room.TenantId && candidate.Id == room.Id);

            Assert.Equal(room.Id, restored.Id);
            Assert.Equal(room.TenantId, restored.TenantId);
            Assert.Equal("A101", restored.Number.Value);
            Assert.Equal(room.Condition, restored.Condition);
        }
    }
}
