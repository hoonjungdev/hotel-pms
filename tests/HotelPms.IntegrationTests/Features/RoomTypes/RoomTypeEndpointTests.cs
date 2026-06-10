using System.Net;
using System.Net.Http.Json;
using HotelPms.Features.RoomTypes;
using HotelPms.Features.RoomTypes.CreateRoomType;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HotelPms.IntegrationTests.Features.RoomTypes;

[Collection(IntegrationTestCollection.Name)]
public class RoomTypeEndpointTests
{
    private const string _tenantHeaderName = "X-Tenant-Id";
    private readonly PostgreSqlFixture _fixture;

    public RoomTypeEndpointTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Post_ValidRequest_PersistsRoomType()
    {
        var tenantId = TenantId.New();
        var request = new CreateRoomTypeRequest(" dbl ", "  Double  ", 2, 4);

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/room-types", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        RoomTypeResponse? body = await response.Content.ReadFromJsonAsync<RoomTypeResponse>();

        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal("DBL", body.Code);
        Assert.Equal("Double", body.Name);
        Assert.Equal(2, body.BaseOccupancy);
        Assert.Equal(4, body.MaxOccupancy);

        await using HotelDbContext context = _fixture.CreateDbContext();
        RoomType roomType = await context.Set<RoomType>().SingleAsync(candidate => candidate.Id == new RoomTypeId(body.Id));

        Assert.Equal(tenantId, roomType.TenantId);
        Assert.Equal("DBL", roomType.Code.Value);
        Assert.Equal("Double", roomType.Name);

        string? location = response.Headers.Location?.ToString();

        Assert.NotNull(location);
        RoomTypeResponse? createdRoomType = await client.GetFromJsonAsync<RoomTypeResponse>(location);

        Assert.NotNull(createdRoomType);
        Assert.Equal(body.Id, createdRoomType.Id);
        Assert.Equal("DBL", createdRoomType.Code);
    }

    [Fact]
    public async Task Get_SameTenantRoomTypes_ReturnsRoomTypesOrderedByCode()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();

        var deluxe = RoomType.Create(tenantId, RoomTypeCode.Create("DLX"), "Deluxe", 2, 4);
        var doubleRoom = RoomType.Create(tenantId, RoomTypeCode.Create("DBL"), "Double", 2, 2);
        var excluded = RoomType.Create(otherTenantId, RoomTypeCode.Create("SGL"), "Single", 1, 1);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().AddRange(deluxe, doubleRoom, excluded);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        RoomTypeResponse[]? response = await client.GetFromJsonAsync<RoomTypeResponse[]>("/api/room-types");

        Assert.NotNull(response);
        Assert.Equal(2, response.Length);
        Assert.Equal("DBL", response[0].Code);
        Assert.Equal("DLX", response[1].Code);
    }

    [Fact]
    public async Task Get_DifferentTenantRoomType_ReturnsNotFound()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        var roomType = RoomType.Create(otherTenantId, RoomTypeCode.Create("DBL"), "Double", 2, 4);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.GetAsync($"/api/room-types/{roomType.Id.Value}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
