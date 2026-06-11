using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.Reservations.Domain;

public sealed record ReservationAvailability(
    RoomTypeId RoomTypeId,
    DateRange StayPeriod,
    int SellableRoomCount,
    int ActiveReservationCount)
{
    public int AvailableRoomCount => Math.Max(0, SellableRoomCount - ActiveReservationCount);
    public bool HasAvailability => AvailableRoomCount > 0;
}
