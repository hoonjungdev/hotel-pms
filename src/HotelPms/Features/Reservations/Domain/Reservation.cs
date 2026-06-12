using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Rooms.Domain;
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
    public Money TotalAmount { get; private set; }
    public RoomId? AssignedRoomId { get; private set; }
    public ReservationStatus Status { get; private set; }

    private Reservation() { }

    private Reservation(
        TenantId tenantId,
        GuestId primaryGuestId,
        RoomTypeId roomTypeId,
        DateRange stayPeriod,
        int guestCount,
        Money totalAmount)
    {
        Id = ReservationId.New();
        TenantId = tenantId;
        PrimaryGuestId = primaryGuestId;
        RoomTypeId = roomTypeId;
        StayPeriod = stayPeriod;
        GuestCount = guestCount;
        TotalAmount = totalAmount;
        Status = ReservationStatus.Pending;
    }

    public static Reservation Create(
        TenantId tenantId,
        GuestId primaryGuestId,
        RoomTypeId roomTypeId,
        DateRange stayPeriod,
        int guestCount,
        Money totalAmount)
    {
        EnsureValidTenantId(tenantId);
        EnsureValidPrimaryGuestId(primaryGuestId);
        EnsureValidRoomTypeId(roomTypeId);
        EnsureValidGuestCount(guestCount);
        EnsureValidTotalAmount(totalAmount);

        var reservation = new Reservation(
            tenantId,
            primaryGuestId,
            roomTypeId,
            stayPeriod,
            guestCount,
            totalAmount);

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

        if (Status == ReservationStatus.CheckedIn)
        {
            throw new InvalidOperationException("In-house reservations cannot be cancelled.");
        }

        Status = ReservationStatus.Cancelled;
    }

    public void CheckIn(Room room)
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed reservations can be checked in.");
        }

        if (room.TenantId != TenantId)
        {
            throw new InvalidOperationException("Assigned room must belong to the reservation tenant.");
        }

        if (room.RoomTypeId != RoomTypeId)
        {
            throw new InvalidOperationException("Assigned room must match the reservation room type.");
        }

        if (room.Condition != RoomCondition.Clean)
        {
            throw new InvalidOperationException("Only clean rooms can be assigned at check-in.");
        }

        AssignedRoomId = room.Id;
        Status = ReservationStatus.CheckedIn;
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

    private static void EnsureValidTotalAmount(Money totalAmount)
    {
        if (!Enum.IsDefined(totalAmount.Currency))
        {
            throw new ArgumentException("Total amount currency must be supported.", nameof(totalAmount));
        }
    }
}
