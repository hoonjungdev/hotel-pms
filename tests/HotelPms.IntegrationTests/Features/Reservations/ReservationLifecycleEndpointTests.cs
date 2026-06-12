using System.Net;
using System.Net.Http.Json;
using HotelPms.Features.Guests;
using HotelPms.Features.Guests.RegisterGuest;
using HotelPms.Features.Reservations;
using HotelPms.Features.Reservations.CheckInReservation;
using HotelPms.Features.Reservations.CreateReservation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms;
using HotelPms.Features.Rooms.AddRoom;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.UpdateRoomCondition;
using HotelPms.Features.RoomTypes;
using HotelPms.Features.RoomTypes.CreateRoomType;
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
public class ReservationLifecycleEndpointTests
{
    private const string _tenantHeaderName = "X-Tenant-Id";
    private readonly PostgreSqlFixture _fixture;

    public ReservationLifecycleEndpointTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReservationLifecycle_ValidStay_CompletesCorePmsFlow()
    {
        var tenantId = TenantId.New();

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        RoomTypeResponse roomType = await PostJsonAsync<RoomTypeResponse>(
            client,
            "/api/room-types",
            new CreateRoomTypeRequest("dbl", "Double", 2, 3, 120_000, "KRW"),
            HttpStatusCode.Created);

        RoomResponse room = await PostJsonAsync<RoomResponse>(
            client,
            "/api/rooms",
            new AddRoomRequest(roomType.Id, "201"),
            HttpStatusCode.Created);

        GuestResponse guest = await PostJsonAsync<GuestResponse>(
            client,
            "/api/guests",
            new RegisterGuestRequest("Jane Doe", "jane.lifecycle@example.com", null),
            HttpStatusCode.Created);

        ReservationResponse createdReservation = await PostJsonAsync<ReservationResponse>(
            client,
            "/api/reservations",
            new CreateReservationRequest(
                guest.Id,
                roomType.Id,
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 3),
                GuestCount: 2),
            HttpStatusCode.Created);

        Assert.Equal(ReservationStatus.Pending.ToString(), createdReservation.Status);
        Assert.Null(createdReservation.AssignedRoomId);
        Assert.Equal(240_000, createdReservation.TotalAmount);
        Assert.Equal("KRW", createdReservation.TotalCurrency);

        ReservationResponse confirmedReservation = await PostJsonAsync<ReservationResponse>(
            client,
            $"/api/reservations/{createdReservation.Id}/confirm",
            content: null,
            HttpStatusCode.OK);

        Assert.Equal(ReservationStatus.Confirmed.ToString(), confirmedReservation.Status);

        ReservationResponse checkedInReservation = await PostJsonAsync<ReservationResponse>(
            client,
            $"/api/reservations/{createdReservation.Id}/check-in",
            new CheckInReservationRequest(room.Id),
            HttpStatusCode.OK);

        Assert.Equal(ReservationStatus.CheckedIn.ToString(), checkedInReservation.Status);
        Assert.Equal(room.Id, checkedInReservation.AssignedRoomId);

        ReservationResponse checkedOutReservation = await PostJsonAsync<ReservationResponse>(
            client,
            $"/api/reservations/{createdReservation.Id}/check-out",
            content: null,
            HttpStatusCode.OK);

        Assert.Equal(ReservationStatus.CheckedOut.ToString(), checkedOutReservation.Status);
        Assert.Equal(room.Id, checkedOutReservation.AssignedRoomId);

        RoomResponse dirtyRoom = await client.GetFromJsonAsync<RoomResponse>($"/api/rooms/{room.Id}") ??
            throw new InvalidOperationException("Room response was empty.");

        Assert.Equal(RoomCondition.Dirty.ToString(), dirtyRoom.Condition);

        HttpResponseMessage cleanRoomResponse = await client.PatchAsJsonAsync(
            $"/api/rooms/{room.Id}/condition",
            new UpdateRoomConditionRequest("Clean"));

        Assert.Equal(HttpStatusCode.NoContent, cleanRoomResponse.StatusCode);

        RoomResponse cleanRoom = await client.GetFromJsonAsync<RoomResponse>($"/api/rooms/{room.Id}") ??
            throw new InvalidOperationException("Room response was empty.");

        Assert.Equal(RoomCondition.Clean.ToString(), cleanRoom.Condition);
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

    private static async Task<TResponse> PostJsonAsync<TResponse>(
        HttpClient client,
        string requestUri,
        object? content,
        HttpStatusCode expectedStatusCode)
    {
        HttpResponseMessage response = content is null
            ? await client.PostAsync(requestUri, content: null)
            : await client.PostAsJsonAsync(requestUri, content);

        Assert.Equal(expectedStatusCode, response.StatusCode);

        TResponse? body = await response.Content.ReadFromJsonAsync<TResponse>();

        return body ?? throw new InvalidOperationException("Response body was empty.");
    }
}
