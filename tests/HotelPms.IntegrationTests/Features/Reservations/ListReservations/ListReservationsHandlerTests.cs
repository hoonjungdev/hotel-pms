using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Reservations.ListReservations;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Reservations.ListReservations;

[Collection(IntegrationTestCollection.Name)]
public class ListReservationsHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public ListReservationsHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_SameTenantReservations_ReturnsReservationsOrderedByCheckInDate()
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

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListReservationsHandler(context);

            List<ReservationListItem> reservations = await handler.HandleAsync(
                new ListReservationsQuery(tenantId));

            Assert.Equal(2, reservations.Count);
            Assert.Equal(earlier.Id, reservations[0].Id);
            Assert.Equal(later.Id, reservations[1].Id);
            Assert.Equal(new Money(240_000, Currency.KRW), reservations[0].TotalAmount);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantReservation_IsExcludedFromResults()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        Guest guest = ReservationTestData.CreateGuest(tenantId);
        RoomType roomType = ReservationTestData.CreateRoomType(tenantId);
        Guest otherGuest = ReservationTestData.CreateGuest(otherTenantId);
        RoomType otherRoomType = ReservationTestData.CreateRoomType(otherTenantId);
        Reservation included = ReservationTestData.CreateReservation(tenantId, guest, roomType);
        Reservation excluded = ReservationTestData.CreateReservation(otherTenantId, otherGuest, otherRoomType);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().AddRange(guest, otherGuest);
            context.Set<RoomType>().AddRange(roomType, otherRoomType);
            context.Set<Reservation>().AddRange(included, excluded);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListReservationsHandler(context);

            List<ReservationListItem> reservations = await handler.HandleAsync(
                new ListReservationsQuery(tenantId));

            ReservationListItem reservation = Assert.Single(reservations);
            Assert.Equal(included.Id, reservation.Id);
        }
    }
}
