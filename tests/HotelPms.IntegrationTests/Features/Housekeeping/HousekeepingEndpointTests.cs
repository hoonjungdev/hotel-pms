using System.Net;
using System.Net.Http.Json;
using HotelPms.Features.Housekeeping;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Features.Rooms;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HotelPms.IntegrationTests.Features.Housekeeping;

[Collection(IntegrationTestCollection.Name)]
public class HousekeepingEndpointTests
{
    private const string _tenantHeaderName = "X-Tenant-Id";
    private readonly PostgreSqlFixture _fixture;

    public HousekeepingEndpointTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetRooms_SameTenantRooms_ReturnsHousekeepingRooms()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        RoomType otherTenantRoomType = RoomTestData.CreateRoomType(otherTenantId);
        Room cleanRoom = RoomTestData.CreateRoom(tenantId, roomType, "A102");
        Room dirtyRoom = RoomTestData.CreateRoom(tenantId, roomType, "A101");
        Room excludedRoom = RoomTestData.CreateRoom(otherTenantId, otherTenantRoomType, "A100");

        dirtyRoom.MarkDirty();
        excludedRoom.MarkDirty();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().AddRange(roomType, otherTenantRoomType);
            context.Set<Room>().AddRange(cleanRoom, dirtyRoom, excludedRoom);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HousekeepingRoomResponse[]? response = await client.GetFromJsonAsync<HousekeepingRoomResponse[]>(
            "/api/housekeeping/rooms");

        Assert.NotNull(response);
        Assert.Equal(2, response.Length);
        Assert.Equal(dirtyRoom.Id.Value, response[0].Id);
        Assert.Equal(RoomCondition.Dirty.ToString(), response[0].Condition);
        Assert.Equal(cleanRoom.Id.Value, response[1].Id);
        Assert.Equal(RoomCondition.Clean.ToString(), response[1].Condition);
    }

    [Fact]
    public async Task GetRooms_DirtyCondition_ReturnsOnlyDirtyRooms()
    {
        var tenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        Room cleanRoom = RoomTestData.CreateRoom(tenantId, roomType, "A101");
        Room dirtyRoom = RoomTestData.CreateRoom(tenantId, roomType, "A102");

        dirtyRoom.MarkDirty();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().AddRange(cleanRoom, dirtyRoom);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HousekeepingRoomResponse[]? response = await client.GetFromJsonAsync<HousekeepingRoomResponse[]>(
            "/api/housekeeping/rooms?condition=Dirty");

        Assert.NotNull(response);
        HousekeepingRoomResponse room = Assert.Single(response);
        Assert.Equal(dirtyRoom.Id.Value, room.Id);
        Assert.Equal(RoomCondition.Dirty.ToString(), room.Condition);
    }

    [Fact]
    public async Task PostMarkClean_DirtyRoom_MarksRoomClean()
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

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsync($"/api/housekeeping/rooms/{room.Id.Value}/mark-clean", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        Room restored = await restoredContext.Set<Room>().SingleAsync(candidate => candidate.Id == room.Id);

        Assert.Equal(RoomCondition.Clean, restored.Condition);
    }

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<HotelDbContext>>();
                    services.AddDbContext<HotelDbContext>(options => options.UseNpgsql(_fixture.ConnectionString));
                });
            });
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory, TenantId tenantId)
    {
        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        client.DefaultRequestHeaders.Add(_tenantHeaderName, tenantId.Value.ToString());

        return client;
    }
}
