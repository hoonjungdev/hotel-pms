using HotelPms.Features.Guests.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.Domain;

public class Reservation : AggregateRoot
{
    public ReservationId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public GuestId PrimaryGuestId { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }
    public DateRange StayPeriod { get; private set; }
    public int GuestCount { get; private set; }
    public ReservationStatus Status { get; private set; }

    private Reservation() { }

    private Reservation(
        TenantId tenantId,
        GuestId primaryGuestId,
        RoomTypeId roomTypeId,
        DateRange stayPeriod,
        int guestCount)
    {
        Id = ReservationId.New();
        TenantId = tenantId;
        PrimaryGuestId = primaryGuestId;
        RoomTypeId = roomTypeId;
        StayPeriod = stayPeriod;
        GuestCount = guestCount;
        Status = ReservationStatus.Pending;
    }

    public static Reservation Create(
        TenantId tenantId,
        GuestId primaryGuestId,
        RoomTypeId roomTypeId,
        DateRange stayPeriod,
        int guestCount)
    {
        EnsureValidTenantId(tenantId);
        EnsureValidPrimaryGuestId(primaryGuestId);
        EnsureValidRoomTypeId(roomTypeId);
        EnsureValidGuestCount(guestCount);

        var reservation = new Reservation(
            tenantId,
            primaryGuestId,
            roomTypeId,
            stayPeriod,
            guestCount);

        reservation.RaiseDomainEvent(new ReservationCreated(
            reservation.Id,
            reservation.TenantId,
            reservation.PrimaryGuestId,
            reservation.RoomTypeId));

        return reservation;
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending reservations can be confirmed.");
        }

        Status = ReservationStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new InvalidOperationException("Reservation is already cancelled.");
        }

        Status = ReservationStatus.Cancelled;
    }

    private static void EnsureValidTenantId(TenantId tenantId)
    {
        if (tenantId.Value == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID must be provided.", nameof(tenantId));
        }
    }

    private static void EnsureValidPrimaryGuestId(GuestId primaryGuestId)
    {
        if (primaryGuestId.Value == Guid.Empty)
        {
            throw new ArgumentException("Primary guest ID must be provided.", nameof(primaryGuestId));
        }
    }

    private static void EnsureValidRoomTypeId(RoomTypeId roomTypeId)
    {
        if (roomTypeId.Value == Guid.Empty)
        {
            throw new ArgumentException("Room type ID must be provided.", nameof(roomTypeId));
        }
    }

    private static void EnsureValidGuestCount(int guestCount)
    {
        if (guestCount < 1)
        {
            throw new ArgumentException("Guest count must be at least 1.", nameof(guestCount));
        }
    }
}
