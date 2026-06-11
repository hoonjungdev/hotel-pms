using System.Net;
using System.Net.Http.Json;
using HotelPms.Features.Rooms;
using HotelPms.Features.Rooms.AddRoom;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.Rooms.UpdateRoomCondition;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HotelPms.IntegrationTests.Features.Rooms;

[Collection(IntegrationTestCollection.Name)]
public class RoomEndpointTests
{
    private const string _tenantHeaderName = "X-Tenant-Id";
    private readonly PostgreSqlFixture _fixture;

    public RoomEndpointTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Post_ValidRequest_PersistsRoom()
    {
        var tenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        var request = new AddRoomRequest(roomType.Id.Value, " a101 ");

        await using (HotelDbContext setupContext = _fixture.CreateDbContext())
        {
            setupContext.Set<RoomType>().Add(roomType);
            await setupContext.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/rooms", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        RoomResponse? body = await response.Content.ReadFromJsonAsync<RoomResponse>();

        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(roomType.Id.Value, body.RoomTypeId);
        Assert.Equal("A101", body.Number);
        Assert.Equal(RoomCondition.Clean.ToString(), body.Condition);

        await using HotelDbContext context = _fixture.CreateDbContext();
        Room room = await context.Set<Room>().SingleAsync(candidate => candidate.Id == new RoomId(body.Id));

        Assert.Equal(tenantId, room.TenantId);
        Assert.Equal(roomType.Id, room.RoomTypeId);
        Assert.Equal("A101", room.Number.Value);

        string? location = response.Headers.Location?.ToString();

        Assert.NotNull(location);
        RoomResponse? createdRoom = await client.GetFromJsonAsync<RoomResponse>(location);

        Assert.NotNull(createdRoom);
        Assert.Equal(body.Id, createdRoom.Id);
    }

    [Fact]
    public async Task Get_SameTenantRooms_ReturnsRoomsOrderedByNumber()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        RoomType otherTenantRoomType = RoomTestData.CreateRoomType(otherTenantId);

        var first = Room.Create(tenantId, roomType.Id, RoomNumber.Create("A102"));
        var second = Room.Create(tenantId, roomType.Id, RoomNumber.Create("A101"));
        var excluded = Room.Create(otherTenantId, otherTenantRoomType.Id, RoomNumber.Create("A100"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().AddRange(roomType, otherTenantRoomType);
            context.Set<Room>().AddRange(first, second, excluded);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        RoomResponse[]? response = await client.GetFromJsonAsync<RoomResponse[]>("/api/rooms");

        Assert.NotNull(response);
        Assert.Equal(2, response.Length);
        Assert.Equal("A101", response[0].Number);
        Assert.Equal(roomType.Id.Value, response[0].RoomTypeId);
        Assert.Equal("A102", response[1].Number);
        Assert.Equal(roomType.Id.Value, response[1].RoomTypeId);
    }

    [Fact]
    public async Task Get_DifferentTenantRoom_ReturnsNotFound()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        RoomType otherTenantRoomType = RoomTestData.CreateRoomType(otherTenantId);
        var room = Room.Create(otherTenantId, otherTenantRoomType.Id, RoomNumber.Create("A101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(otherTenantRoomType);
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.GetAsync($"/api/rooms/{room.Id.Value}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchCondition_ExistingRoom_PersistsCondition()
    {
        var tenantId = TenantId.New();
        RoomType roomType = RoomTestData.CreateRoomType(tenantId);
        var room = Room.Create(tenantId, roomType.Id, RoomNumber.Create("A101"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PatchAsJsonAsync(
            $"/api/rooms/{room.Id.Value}/condition",
            new UpdateRoomConditionRequest("Dirty"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        Room restored = await restoredContext.Set<Room>().SingleAsync(candidate => candidate.Id == room.Id);

        Assert.Equal(RoomCondition.Dirty, restored.Condition);
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
