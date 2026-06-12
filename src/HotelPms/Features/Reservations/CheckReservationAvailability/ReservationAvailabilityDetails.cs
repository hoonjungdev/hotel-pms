using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.Reservations.CheckReservationAvailability;

public sealed record ReservationAvailabilityDetails(
    RoomTypeId RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int SellableRoomCount,
    int ActiveReservationCount,
    int AvailableRoomCount,
    bool HasAvailability);
