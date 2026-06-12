using System.Net;
using System.Net.Http.Json;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations;
using HotelPms.Features.Reservations.CheckReservationAvailability;
using HotelPms.Features.Reservations.CreateReservation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
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
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
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
            context.Set<Room>().Add(room);
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

    [Fact]
    public async Task Post_OverlappingReservationConsumesOnlyRoom_ReturnsBadRequest()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        Reservation existingReservation = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 7, 1),
            checkOutDate: new DateOnly(2026, 7, 3));
        var request = new CreateReservationRequest(
            guest.Id.Value,
            roomType.Id.Value,
            new DateOnly(2026, 7, 2),
            new DateOnly(2026, 7, 4),
            GuestCount: 2);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().Add(room);
            context.Set<Reservation>().Add(existingReservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_OutOfServiceRoomAndCancelledReservation_ReturnsAvailabilityCounts()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room firstRoom = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        Room secondRoom = ReservationTestData.CreateRoom(tenantId, roomType, "A102");
        Room outOfServiceRoom = ReservationTestData.CreateRoom(tenantId, roomType, "A103");
        outOfServiceRoom.TakeOutOfService();
        Reservation activeReservation = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 7, 1),
            checkOutDate: new DateOnly(2026, 7, 3));
        Reservation cancelledReservation = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 7, 1),
            checkOutDate: new DateOnly(2026, 7, 3));
        cancelledReservation.Cancel();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().AddRange(firstRoom, secondRoom, outOfServiceRoom);
            context.Set<Reservation>().AddRange(activeReservation, cancelledReservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        CheckReservationAvailabilityResponse? response =
            await client.GetFromJsonAsync<CheckReservationAvailabilityResponse>(
                $"/api/reservations/availability?roomTypeId={roomType.Id.Value}" +
                "&checkInDate=2026-07-02&checkOutDate=2026-07-04");

        Assert.NotNull(response);
        Assert.Equal(roomType.Id.Value, response.RoomTypeId);
        Assert.Equal(new DateOnly(2026, 7, 2), response.CheckInDate);
        Assert.Equal(new DateOnly(2026, 7, 4), response.CheckOutDate);
        Assert.Equal(2, response.SellableRoomCount);
        Assert.Equal(1, response.ActiveReservationCount);
        Assert.Equal(1, response.AvailableRoomCount);
        Assert.True(response.HasAvailability);
    }

    [Fact]
    public async Task GetAvailability_DifferentTenantRoomType_ReturnsBadRequest()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        RoomType roomType = ReservationTestData.CreateRoomType(otherTenantId);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.GetAsync(
            $"/api/reservations/availability?roomTypeId={roomType.Id.Value}" +
            "&checkInDate=2026-07-02&checkOutDate=2026-07-04");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_CheckOutDateBeforeCheckInDate_ReturnsBadRequest()
    {
        var tenantId = TenantId.New();
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<RoomType>().Add(roomType);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.GetAsync(
            $"/api/reservations/availability?roomTypeId={roomType.Id.Value}" +
            "&checkInDate=2026-07-04&checkOutDate=2026-07-02");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_SameTenantReservations_ReturnsReservationsOrderedByCheckInDate()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Reservation later = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 8, 1),
            checkOutDate: new DateOnly(2026, 8, 3));
        Reservation earlier = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 7, 1),
            checkOutDate: new DateOnly(2026, 7, 3));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().AddRange(later, earlier);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        ReservationResponse[]? response = await client.GetFromJsonAsync<ReservationResponse[]>("/api/reservations");

        Assert.NotNull(response);
        Assert.Equal(2, response.Length);
        Assert.Equal(earlier.Id.Value, response[0].Id);
        Assert.Equal(later.Id.Value, response[1].Id);
    }

    [Fact]
    public async Task Get_ExistingReservation_ReturnsReservation()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        ReservationResponse? response = await client.GetFromJsonAsync<ReservationResponse>(
            $"/api/reservations/{reservation.Id.Value}");

        Assert.NotNull(response);
        Assert.Equal(reservation.Id.Value, response.Id);
        Assert.Equal(guest.Id.Value, response.PrimaryGuestId);
        Assert.Equal(roomType.Id.Value, response.RoomTypeId);
        Assert.Equal(new DateOnly(2026, 7, 1), response.CheckInDate);
        Assert.Equal(new DateOnly(2026, 7, 3), response.CheckOutDate);
        Assert.Equal(2, response.GuestCount);
        Assert.Equal(ReservationStatus.Pending.ToString(), response.Status);
    }

    [Fact]
    public async Task Get_DifferentTenantReservation_ReturnsNotFound()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(otherTenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(otherTenantId);
        Reservation reservation = ReservationTestData.CreateReservation(otherTenantId, guest, roomType);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.GetAsync($"/api/reservations/{reservation.Id.Value}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostConfirm_PendingReservation_ReturnsConfirmedReservation()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsync($"/api/reservations/{reservation.Id.Value}/confirm", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ReservationResponse? body = await response.Content.ReadFromJsonAsync<ReservationResponse>();

        Assert.NotNull(body);
        Assert.Equal(reservation.Id.Value, body.Id);
        Assert.Equal(ReservationStatus.Confirmed.ToString(), body.Status);

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        Reservation restoredReservation = await restoredContext.Set<Reservation>()
            .SingleAsync(candidate => candidate.Id == reservation.Id);

        Assert.Equal(ReservationStatus.Confirmed, restoredReservation.Status);
    }

    [Fact]
    public async Task PostConfirm_DifferentTenantReservation_ReturnsNotFound()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(otherTenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(otherTenantId);
        Reservation reservation = ReservationTestData.CreateReservation(otherTenantId, guest, roomType);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsync($"/api/reservations/{reservation.Id.Value}/confirm", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostConfirm_CancelledReservation_ReturnsBadRequest()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        reservation.Cancel();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsync($"/api/reservations/{reservation.Id.Value}/confirm", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostCancel_ConfirmedReservation_ReturnsCancelledReservation()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        reservation.Confirm();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsync($"/api/reservations/{reservation.Id.Value}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ReservationResponse? body = await response.Content.ReadFromJsonAsync<ReservationResponse>();

        Assert.NotNull(body);
        Assert.Equal(reservation.Id.Value, body.Id);
        Assert.Equal(ReservationStatus.Cancelled.ToString(), body.Status);
    }

    [Fact]
    public async Task PostCancel_DifferentTenantReservation_ReturnsNotFound()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(otherTenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(otherTenantId);
        Reservation reservation = ReservationTestData.CreateReservation(otherTenantId, guest, roomType);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsync($"/api/reservations/{reservation.Id.Value}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostCancel_CancelledReservation_ReturnsBadRequest()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        reservation.Cancel();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsync($"/api/reservations/{reservation.Id.Value}/cancel", null);

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
