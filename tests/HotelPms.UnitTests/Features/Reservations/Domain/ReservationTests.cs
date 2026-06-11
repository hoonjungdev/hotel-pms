using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.UnitTests.Features.Reservations.Domain;

public class ReservationTests
{
    [Fact]
    public void Create_ValidReservation_ReturnsPendingReservation()
    {
        var tenantId = TenantId.New();
        var primaryGuestId = GuestId.New();
        var roomTypeId = RoomTypeId.New();
        var stayPeriod = new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3));

        var reservation = Reservation.Create(
            tenantId,
            primaryGuestId,
            roomTypeId,
            stayPeriod,
            guestCount: 2);

        Assert.Equal(tenantId, reservation.TenantId);
        Assert.Equal(primaryGuestId, reservation.PrimaryGuestId);
        Assert.Equal(roomTypeId, reservation.RoomTypeId);
        Assert.Equal(stayPeriod, reservation.StayPeriod);
        Assert.Equal(2, reservation.GuestCount);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public void Create_ValidReservation_RaisesReservationCreatedEvent()
    {
        var tenantId = TenantId.New();
        var primaryGuestId = GuestId.New();
        var roomTypeId = RoomTypeId.New();

        var reservation = Reservation.Create(
            tenantId,
            primaryGuestId,
            roomTypeId,
            new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3)),
            guestCount: 2);

        ReservationCreated domainEvent = Assert.IsType<ReservationCreated>(
            Assert.Single(reservation.DomainEvents));

        Assert.Equal(reservation.Id, domainEvent.ReservationId);
        Assert.Equal(tenantId, domainEvent.TenantId);
        Assert.Equal(primaryGuestId, domainEvent.PrimaryGuestId);
        Assert.Equal(roomTypeId, domainEvent.RoomTypeId);
    }

    [Fact]
    public void Create_EmptyTenantId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            Reservation.Create(
                new TenantId(Guid.Empty),
                GuestId.New(),
                RoomTypeId.New(),
                new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3)),
                guestCount: 2));
    }

    [Fact]
    public void Create_EmptyPrimaryGuestId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            Reservation.Create(
                TenantId.New(),
                new GuestId(Guid.Empty),
                RoomTypeId.New(),
                new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3)),
                guestCount: 2));
    }

    [Fact]
    public void Create_EmptyRoomTypeId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            Reservation.Create(
                TenantId.New(),
                GuestId.New(),
                new RoomTypeId(Guid.Empty),
                new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3)),
                guestCount: 2));
    }

    [Fact]
    public void Create_GuestCountLessThanOne_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            Reservation.Create(
                TenantId.New(),
                GuestId.New(),
                RoomTypeId.New(),
                new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3)),
                guestCount: 0));
    }
}
