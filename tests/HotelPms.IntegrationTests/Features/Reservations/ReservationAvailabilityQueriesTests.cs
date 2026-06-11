using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Reservations.Infrastructure;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Reservations;

[Collection(IntegrationTestCollection.Name)]
public class ReservationAvailabilityQueriesTests
{
    private readonly PostgreSqlFixture _fixture;

    public ReservationAvailabilityQueriesTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetReservationAvailabilityAsync_OutOfServiceRoomAndCancelledReservation_ReturnsAvailabilityCounts()
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

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().AddRange(firstRoom, secondRoom, outOfServiceRoom);
        context.Set<Reservation>().AddRange(activeReservation, cancelledReservation);
        await context.SaveChangesAsync();

        ReservationAvailability availability = await context.GetReservationAvailabilityAsync(
            tenantId,
            roomType.Id,
            new DateRange(new DateOnly(2026, 7, 2), new DateOnly(2026, 7, 4)));

        Assert.Equal(roomType.Id, availability.RoomTypeId);
        Assert.Equal(2, availability.SellableRoomCount);
        Assert.Equal(1, availability.ActiveReservationCount);
        Assert.Equal(1, availability.AvailableRoomCount);
        Assert.True(availability.HasAvailability);
    }

    [Fact]
    public async Task GetReservationAvailabilityAsync_ConfirmedOverlappingReservation_CountsAsActiveReservation()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "B101");
        Reservation confirmedReservation = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 7, 1),
            checkOutDate: new DateOnly(2026, 7, 3));
        confirmedReservation.Confirm();

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(confirmedReservation);
        await context.SaveChangesAsync();

        ReservationAvailability availability = await context.GetReservationAvailabilityAsync(
            tenantId,
            roomType.Id,
            new DateRange(new DateOnly(2026, 7, 2), new DateOnly(2026, 7, 4)));

        Assert.Equal(1, availability.SellableRoomCount);
        Assert.Equal(1, availability.ActiveReservationCount);
        Assert.False(availability.HasAvailability);
    }
}
