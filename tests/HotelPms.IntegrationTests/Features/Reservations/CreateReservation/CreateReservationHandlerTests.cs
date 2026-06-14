using FluentValidation;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.CreateReservation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Reservations.CreateReservation;

[Collection(IntegrationTestCollection.Name)]
public class CreateReservationHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public CreateReservationHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_PersistsReservation()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3),
            GuestCount: 2);

        CreateReservationResult result;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            context.Set<RoomType>().Add(roomType);
            context.Set<Room>().Add(room);
            await context.SaveChangesAsync();

            CreateReservationHandler handler = CreateHandler(context);

            result = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Reservation reservation = await context.Set<Reservation>()
                .SingleAsync(candidate => candidate.Id == result.Id);

            Assert.Equal(tenantId, reservation.TenantId);
            Assert.Equal(guest.Id, reservation.PrimaryGuestId);
            Assert.Equal(roomType.Id, reservation.RoomTypeId);
            Assert.Equal(new DateOnly(2026, 7, 1), reservation.StayPeriod.Start);
            Assert.Equal(new DateOnly(2026, 7, 3), reservation.StayPeriod.End);
            Assert.Equal(2, reservation.GuestCount);
            Assert.Equal(new Money(240_000, Currency.KRW), reservation.TotalAmount);
            Assert.Equal(ReservationStatus.Pending, reservation.Status);
        }

        Assert.Equal(guest.Id, result.PrimaryGuestId);
        Assert.Equal(roomType.Id, result.RoomTypeId);
        Assert.Equal(new Money(240_000, Currency.KRW), result.TotalAmount);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantGuest_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(otherTenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3),
            GuestCount: 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        await context.SaveChangesAsync();

        CreateReservationHandler handler = CreateHandler(context);

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantRoomType_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(otherTenantId);
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3),
            GuestCount: 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        await context.SaveChangesAsync();

        CreateReservationHandler handler = CreateHandler(context);

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_GuestCountExceedsMaxOccupancy_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId, maxOccupancy: 2);
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3),
            GuestCount: 3);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        await context.SaveChangesAsync();

        CreateReservationHandler handler = CreateHandler(context);

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_OverlappingReservationConsumesOnlyRoom_ThrowsValidationException()
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
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 2),
            new DateOnly(2026, 7, 4),
            GuestCount: 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(existingReservation);
        await context.SaveChangesAsync();

        CreateReservationHandler handler = CreateHandler(context);

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_OneOfTwoRoomsConsumedByOverlappingReservation_PersistsReservation()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room firstRoom = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        Room secondRoom = ReservationTestData.CreateRoom(tenantId, roomType, "A102");
        Reservation existingReservation = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 7, 1),
            checkOutDate: new DateOnly(2026, 7, 3));
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 2),
            new DateOnly(2026, 7, 4),
            GuestCount: 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().AddRange(firstRoom, secondRoom);
        context.Set<Reservation>().Add(existingReservation);
        await context.SaveChangesAsync();

        CreateReservationHandler handler = CreateHandler(context);

        CreateReservationResult result = await handler.HandleAsync(command);

        Assert.Equal(ReservationStatus.Pending.ToString(), result.Status);
        Assert.Equal(new Money(240_000, Currency.KRW), result.TotalAmount);
    }

    [Fact]
    public async Task HandleAsync_OutOfServiceRoom_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        room.TakeOutOfService();
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3),
            GuestCount: 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        await context.SaveChangesAsync();

        CreateReservationHandler handler = CreateHandler(context);

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_CancelledOverlappingReservation_PersistsReservation()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        Reservation cancelledReservation = ReservationTestData.CreateReservation(
            tenantId,
            guest,
            roomType,
            checkInDate: new DateOnly(2026, 7, 1),
            checkOutDate: new DateOnly(2026, 7, 3));
        cancelledReservation.Cancel();
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 2),
            new DateOnly(2026, 7, 4),
            GuestCount: 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(cancelledReservation);
        await context.SaveChangesAsync();

        CreateReservationHandler handler = CreateHandler(context);

        CreateReservationResult result = await handler.HandleAsync(command);

        Assert.Equal(ReservationStatus.Pending.ToString(), result.Status);
    }

    [Fact]
    public async Task HandleAsync_NonOverlappingReservation_PersistsReservation()
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
        var command = new CreateReservationCommand(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateOnly(2026, 7, 3),
            new DateOnly(2026, 7, 5),
            GuestCount: 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(existingReservation);
        await context.SaveChangesAsync();

        CreateReservationHandler handler = CreateHandler(context);

        CreateReservationResult result = await handler.HandleAsync(command);

        Assert.Equal(ReservationStatus.Pending.ToString(), result.Status);
    }

    [Fact]
    public async Task HandleAsync_CheckOutDateBeforeCheckInDate_ThrowsValidationException()
    {
        var command = new CreateReservationCommand(
            TenantId.New(),
            GuestId.New(),
            RoomTypeId.New(),
            new DateOnly(2026, 7, 3),
            new DateOnly(2026, 7, 1),
            GuestCount: 2);

        await using HotelDbContext context = _fixture.CreateDbContext();
        CreateReservationHandler handler = CreateHandler(context);

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }

    private static CreateReservationHandler CreateHandler(HotelDbContext context)
    {
        return new CreateReservationHandler(
            context,
            new CreateReservationCommandValidator());
    }
}
