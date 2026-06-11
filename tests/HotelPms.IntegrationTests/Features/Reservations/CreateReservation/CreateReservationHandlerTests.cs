using FluentValidation;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.CreateReservation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
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
            await context.SaveChangesAsync();

            var handler = new CreateReservationHandler(context, new CreateReservationCommandValidator());

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
            Assert.Equal(ReservationStatus.Pending, reservation.Status);
        }

        Assert.Equal(guest.Id, result.PrimaryGuestId);
        Assert.Equal(roomType.Id, result.RoomTypeId);
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

        var handler = new CreateReservationHandler(context, new CreateReservationCommandValidator());

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

        var handler = new CreateReservationHandler(context, new CreateReservationCommandValidator());

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

        var handler = new CreateReservationHandler(context, new CreateReservationCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
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
        var handler = new CreateReservationHandler(context, new CreateReservationCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(async () => await handler.HandleAsync(command));
    }
}
