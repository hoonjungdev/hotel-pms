using FluentValidation;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.CheckOutReservation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Reservations.CheckOutReservation;

[Collection(IntegrationTestCollection.Name)]
public class CheckOutReservationHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public CheckOutReservationHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_CheckedInReservation_MarksReservationCheckedOutAndRoomDirty()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Room room = ReservationTestData.CreateRoom(tenantId, roomType, "A101");
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        reservation.Confirm();
        reservation.CheckIn(room);

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
            var handler = new CheckOutReservationHandler(context, new CheckOutReservationCommandValidator());

            CheckOutReservationResult? result = await handler.HandleAsync(
                new CheckOutReservationCommand(tenantId, reservation.Id));

            Assert.NotNull(result);
            Assert.Equal(room.Id, result.AssignedRoomId);
            Assert.Equal(ReservationStatus.CheckedOut.ToString(), result.Status);
        }

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        Reservation restoredReservation = await restoredContext.Set<Reservation>()
            .SingleAsync(candidate => candidate.Id == reservation.Id);
        Room restoredRoom = await restoredContext.Set<Room>()
            .SingleAsync(candidate => candidate.Id == room.Id);

        Assert.Equal(ReservationStatus.CheckedOut, restoredReservation.Status);
        Assert.Equal(RoomCondition.Dirty, restoredRoom.Condition);
    }

    [Fact]
    public async Task HandleAsync_ConfirmedReservation_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        reservation.Confirm();

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync();

        var handler = new CheckOutReservationHandler(context, new CheckOutReservationCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(new CheckOutReservationCommand(tenantId, reservation.Id)));
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantReservation_ReturnsNull()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(otherTenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(otherTenantId);
        Room room = ReservationTestData.CreateRoom(otherTenantId, roomType, "A101");
        Reservation reservation = ReservationTestData.CreateReservation(otherTenantId, guest, roomType);
        reservation.Confirm();
        reservation.CheckIn(room);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Room>().Add(room);
        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync();

        var handler = new CheckOutReservationHandler(context, new CheckOutReservationCommandValidator());

        CheckOutReservationResult? result = await handler.HandleAsync(
            new CheckOutReservationCommand(tenantId, reservation.Id));

        Assert.Null(result);
    }
}
