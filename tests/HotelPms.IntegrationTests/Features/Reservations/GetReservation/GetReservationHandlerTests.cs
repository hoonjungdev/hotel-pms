using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Reservations.GetReservation;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Reservations.GetReservation;

[Collection(IntegrationTestCollection.Name)]
public class GetReservationHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public GetReservationHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_ExistingReservation_ReturnsReservationDetails()
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
            var handler = new GetReservationHandler(context);

            ReservationDetails? details = await handler.HandleAsync(
                new GetReservationQuery(tenantId, reservation.Id));

            Assert.NotNull(details);
            Assert.Equal(reservation.Id, details.Id);
            Assert.Equal(guest.Id, details.PrimaryGuestId);
            Assert.Equal(roomType.Id, details.RoomTypeId);
            Assert.Equal(new DateOnly(2026, 7, 1), details.CheckInDate);
            Assert.Equal(new DateOnly(2026, 7, 3), details.CheckOutDate);
            Assert.Equal(2, details.GuestCount);
            Assert.Equal(new Money(240_000, Currency.KRW), details.TotalAmount);
            Assert.Equal(ReservationStatus.Pending.ToString(), details.Status);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantReservation_ReturnsNull()
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

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new GetReservationHandler(context);

            ReservationDetails? details = await handler.HandleAsync(
                new GetReservationQuery(tenantId, reservation.Id));

            Assert.Null(details);
        }
    }
}
