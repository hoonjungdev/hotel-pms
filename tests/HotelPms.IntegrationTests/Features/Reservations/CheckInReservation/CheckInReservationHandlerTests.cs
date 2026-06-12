using FluentValidation;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.CheckInReservation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Reservations.CheckInReservation;

[Collection(IntegrationTestCollection.Name)]
public class CheckInReservationHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public CheckInReservationHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_ConfirmedReservationAndCleanMatchingRoom_AssignsRoomAndMarksReservationCheckedIn()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        reservation.Confirm();

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().Add(room);
            context.Set<Reservation>().Add(reservation);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new CheckInReservationHandler(context, new CheckInReservationCommandValidator());

            CheckInReservationResult? result = await handler.HandleAsync(
                new CheckInReservationCommand(tenantId, reservation.Id, room.Id));

            Assert.NotNull(result);
            Assert.Equal(room.Id, result.AssignedRoomId);
            Assert.Equal(ReservationStatus.CheckedIn.ToString(), result.Status);
        }

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        Reservation restoredReservation = await restoredContext.Set<Reservation>()
            .SingleAsync(candidate => candidate.Id == reservation.Id);

        Assert.Equal(room.Id, restoredReservation.AssignedRoomId);
        Assert.Equal(ReservationStatus.CheckedIn, restoredReservation.Status);
    }

    [Fact]
    public async Task HandleAsync_PendingReservation_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync();

        var handler = new CheckInReservationHandler(context, new CheckInReservationCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(new CheckInReservationCommand(tenantId, reservation.Id, room.Id)));
    }

    [Fact]
    public async Task HandleAsync_DirtyRoom_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        room.MarkDirty();
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        reservation.Confirm();

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync();

        var handler = new CheckInReservationHandler(context, new CheckInReservationCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(new CheckInReservationCommand(tenantId, reservation.Id, room.Id)));
    }

    [Fact]
    public async Task HandleAsync_DifferentRoomType_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType reservedRoomType = ReservationTestData.CreateRoomType(tenantId);
        RoomType assignedRoomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, assignedRoomType, "A101");
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, reservedRoomType);
        reservation.Confirm();

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().AddRange(reservedRoomType, assignedRoomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync();

        var handler = new CheckInReservationHandler(context, new CheckInReservationCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(new CheckInReservationCommand(tenantId, reservation.Id, room.Id)));
    }

    [Fact]
    public async Task HandleAsync_OverlappingCheckedInReservationForRoom_ThrowsValidationException()
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
        existingReservation.Confirm();
        existingReservation.CheckIn(room);
        Reservation requestedReservation = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 7, 2),
            checkOutDate: new DateOnly(2026, 7, 4));
        requestedReservation.Confirm();

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().AddRange(existingReservation, requestedReservation);
        await context.SaveChangesAsync();

        var handler = new CheckInReservationHandler(context, new CheckInReservationCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(new CheckInReservationCommand(tenantId, requestedReservation.Id, room.Id)));
    }
}
