using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Reservations.Infrastructure;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Reservations;

[Collection(IntegrationTestCollection.Name)]
public class ReservationAvailabilityOperationsTests
{
    private readonly PostgreSqlFixture _fixture;

    public ReservationAvailabilityOperationsTests(PostgreSqlFixture fixture)
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

    [Fact]
    public async Task AddReservationWhenAvailableAsync_NoAvailability_ThrowsNoReservationAvailabilityException()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "C101");
        var stayPeriod = new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3));
        Reservation reservation = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: stayPeriod.Start,
            checkOutDate: stayPeriod.End);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync();

        NoReservationAvailabilityException exception = await Assert.ThrowsAsync<NoReservationAvailabilityException>(
            () => context.AddReservationWhenAvailableAsync(
                tenantId,
                roomType.Id,
                stayPeriod,
                () => ReservationTestData.CreateReservation(tenantId, guest, roomType, stayPeriod.Start, stayPeriod.End)));

        Assert.Equal(roomType.Id, exception.RoomTypeId);
        Assert.Equal(stayPeriod, exception.StayPeriod);
    }

    [Fact]
    public async Task AddReservationWhenAvailableAsync_ConcurrentRequestsForLastRoom_PersistsOnlyOneReservation()
    {
        var tenantId = TenantId.New();
        Guest firstGuest = ReservationTestData.CreateGuest(tenantId, "First Guest");
        Guest secondGuest = ReservationTestData.CreateGuest(tenantId, "Second Guest");
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "D101");
        var stayPeriod = new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().AddRange(firstGuest, secondGuest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();
        }

        await using HotelDbContext firstContext = _fixture.CreateDbContext();
        await using HotelDbContext secondContext = _fixture.CreateDbContext();

        ReservationAttempt[] attempts = await Task.WhenAll(
            CaptureReservationAttemptAsync(() => firstContext.AddReservationWhenAvailableAsync(
                tenantId,
                roomType.Id,
                stayPeriod,
                () => ReservationTestData.CreateReservation(
                    tenantId,
                    firstGuest,
                    roomType,
                    stayPeriod.Start,
                    stayPeriod.End))),
            CaptureReservationAttemptAsync(() => secondContext.AddReservationWhenAvailableAsync(
                tenantId,
                roomType.Id,
                stayPeriod,
                () => ReservationTestData.CreateReservation(
                    tenantId,
                    secondGuest,
                    roomType,
                    stayPeriod.Start,
                    stayPeriod.End))));

        Assert.Single(attempts, attempt => attempt.Result is not null);
        Assert.Single(attempts, attempt => attempt.Exception is NoReservationAvailabilityException);

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        int reservationCount = await restoredContext.Set<Reservation>()
            .CountAsync(candidate =>
                candidate.TenantId == tenantId &&
                candidate.RoomTypeId == roomType.Id &&
                candidate.StayPeriod.Start == stayPeriod.Start &&
                candidate.StayPeriod.End == stayPeriod.End);

        Assert.Equal(1, reservationCount);
    }

    private static async Task<ReservationAttempt> CaptureReservationAttemptAsync(Func<Task<Reservation>> action)
    {
        try
        {
            return new ReservationAttempt(await action(), null);
        }
        catch (Exception exception)
        {
            return new ReservationAttempt(null, exception);
        }
    }

    private sealed record ReservationAttempt(Reservation? Result, Exception? Exception);
}
