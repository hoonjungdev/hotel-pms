using System.Net;
using System.Net.Http.Json;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations;
using HotelPms.Features.Reservations.CreateReservation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HotelPms.IntegrationTests.Features.Reservations;

[Collection(IntegrationTestCollection.Name)]
public class ReservationEndpointTests
{
    private const string _tenantHeaderName = "X-Tenant-Id";
    private readonly PostgreSqlFixture _fixture;

    public ReservationEndpointTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Post_ValidRequest_PersistsReservation()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        var request = new CreateReservationRequest(
            guest.Id.Value,
            roomType.Id.Value,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3),
            GuestCount: 2);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ReservationResponse? body = await response.Content.ReadFromJsonAsync<ReservationResponse>();

        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(guest.Id.Value, body.PrimaryGuestId);
        Assert.Equal(roomType.Id.Value, body.RoomTypeId);
        Assert.Equal(new DateOnly(2026, 7, 1), body.CheckInDate);
        Assert.Equal(new DateOnly(2026, 7, 3), body.CheckOutDate);
        Assert.Equal(2, body.GuestCount);
        Assert.Equal(ReservationStatus.Pending.ToString(), body.Status);

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        Reservation reservation = await restoredContext.Set<Reservation>()
            .SingleAsync(candidate => candidate.Id == new ReservationId(body.Id));

        Assert.Equal(tenantId, reservation.TenantId);
        Assert.Equal(guest.Id, reservation.PrimaryGuestId);
        Assert.Equal(roomType.Id, reservation.RoomTypeId);
    }

    [Fact]
    public async Task Post_DifferentTenantRoomType_ReturnsBadRequest()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(otherTenantId);
        var request = new CreateReservationRequest(
            guest.Id.Value,
            roomType.Id.Value,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3),
            GuestCount: 2);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
