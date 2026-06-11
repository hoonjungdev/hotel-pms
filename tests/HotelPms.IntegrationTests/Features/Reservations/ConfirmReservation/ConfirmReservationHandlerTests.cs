using FluentValidation;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.ConfirmReservation;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Reservations.ConfirmReservation;

[Collection(IntegrationTestCollection.Name)]
public class ConfirmReservationHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public ConfirmReservationHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_PendingReservation_MarksReservationConfirmed()
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

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ConfirmReservationHandler(context);

            ConfirmReservationResult? result = await handler.HandleAsync(
                new ConfirmReservationCommand(tenantId, reservation.Id));

            Assert.NotNull(result);
            Assert.Equal(ReservationStatus.Confirmed.ToString(), result.Status);
        }

        await using HotelDbContext restoredContext = _fixture.CreateDbContext();
        Reservation restoredReservation = await restoredContext.Set<Reservation>()
            .SingleAsync(candidate => candidate.Id == reservation.Id);

        Assert.Equal(ReservationStatus.Confirmed, restoredReservation.Status);
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantReservation_ReturnsNull()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(otherTenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(otherTenantId);
        Reservation reservation = ReservationTestData.CreateReservation(otherTenantId, guest, roomType);

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync();

        var handler = new ConfirmReservationHandler(context);

        ConfirmReservationResult? result = await handler.HandleAsync(
            new ConfirmReservationCommand(tenantId, reservation.Id));

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_CancelledReservation_ThrowsValidationException()
    {
        var tenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Reservation reservation = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        reservation.Cancel();

        await using HotelDbContext context = _fixture.CreateDbContext();
        context.Set<Guest>().Add(guest);
        context.Set<RoomType>().Add(roomType);
        context.Set<Reservation>().Add(reservation);
        await context.SaveChangesAsync();

        var handler = new ConfirmReservationHandler(context);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(new ConfirmReservationCommand(tenantId, reservation.Id)));
    }
}
