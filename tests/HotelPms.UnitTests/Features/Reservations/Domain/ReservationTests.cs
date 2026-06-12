using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.UnitTests.Features.Reservations.Domain;

public class ReservationTests
{
    private static readonly Money _totalAmount = new(240_000, Currency.KRW);

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
            guestCount: 2,
            _totalAmount);

        Assert.Equal(tenantId, reservation.TenantId);
        Assert.Equal(primaryGuestId, reservation.PrimaryGuestId);
        Assert.Equal(roomTypeId, reservation.RoomTypeId);
        Assert.Equal(stayPeriod, reservation.StayPeriod);
        Assert.Equal(2, reservation.GuestCount);
        Assert.Equal(_totalAmount, reservation.TotalAmount);
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
            guestCount: 2,
            _totalAmount);

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
                guestCount: 2,
                _totalAmount));
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
                guestCount: 2,
                _totalAmount));
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
                guestCount: 2,
                _totalAmount));
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
                guestCount: 0,
                _totalAmount));
    }

    [Fact]
    public void Confirm_PendingReservation_MarksReservationConfirmed()
    {
        Reservation reservation = CreateReservation();

        reservation.Confirm();

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    [Fact]
    public void Confirm_ConfirmedReservation_ThrowsException()
    {
        Reservation reservation = CreateReservation();
        reservation.Confirm();

        Assert.Throws<InvalidOperationException>(reservation.Confirm);
    }

    [Fact]
    public void Confirm_CancelledReservation_ThrowsException()
    {
        Reservation reservation = CreateReservation();
        reservation.Cancel();

        Assert.Throws<InvalidOperationException>(reservation.Confirm);
    }

    [Fact]
    public void Cancel_PendingReservation_MarksReservationCancelled()
    {
        Reservation reservation = CreateReservation();

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_ConfirmedReservation_MarksReservationCancelled()
    {
        Reservation reservation = CreateReservation();
        reservation.Confirm();

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_CancelledReservation_ThrowsException()
    {
        Reservation reservation = CreateReservation();
        reservation.Cancel();

        Assert.Throws<InvalidOperationException>(reservation.Cancel);
    }

    [Fact]
    public void Cancel_CheckedInReservation_ThrowsException()
    {
        Reservation reservation = CreateReservation();
        Room room = CreateRoom(reservation.TenantId, reservation.RoomTypeId);
        reservation.Confirm();
        reservation.CheckIn(room);

        Assert.Throws<InvalidOperationException>(reservation.Cancel);
    }

    [Fact]
    public void CheckIn_ConfirmedReservationAndCleanMatchingRoom_MarksReservationCheckedIn()
    {
        Reservation reservation = CreateReservation();
        Room room = CreateRoom(reservation.TenantId, reservation.RoomTypeId);
        reservation.Confirm();

        reservation.CheckIn(room);

        Assert.Equal(ReservationStatus.CheckedIn, reservation.Status);
        Assert.Equal(room.Id, reservation.AssignedRoomId);
    }

    [Fact]
    public void CheckIn_PendingReservation_ThrowsException()
    {
        Reservation reservation = CreateReservation();
        Room room = CreateRoom(reservation.TenantId, reservation.RoomTypeId);

        Assert.Throws<InvalidOperationException>(() => reservation.CheckIn(room));
    }

    [Fact]
    public void CheckIn_RoomTypeMismatch_ThrowsException()
    {
        Reservation reservation = CreateReservation();
        Room room = CreateRoom(reservation.TenantId, RoomTypeId.New());
        reservation.Confirm();

        Assert.Throws<InvalidOperationException>(() => reservation.CheckIn(room));
    }

    [Fact]
    public void CheckIn_DirtyRoom_ThrowsException()
    {
        Reservation reservation = CreateReservation();
        Room room = CreateRoom(reservation.TenantId, reservation.RoomTypeId);
        room.MarkDirty();
        reservation.Confirm();

        Assert.Throws<InvalidOperationException>(() => reservation.CheckIn(room));
    }

    private static Reservation CreateReservation()
    {
        return Reservation.Create(
            TenantId.New(),
            GuestId.New(),
            RoomTypeId.New(),
            new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3)),
            guestCount: 2,
            _totalAmount);
    }

    private static Room CreateRoom(TenantId tenantId, RoomTypeId roomTypeId)
    {
        return Room.Create(tenantId, roomTypeId, RoomNumber.Create($"R{Guid.NewGuid():N}"[..20]));
    }
}
