namespace HotelPms.Features.Reservations.CheckReservationAvailability;

public sealed record CheckReservationAvailabilityResponse(
    Guid RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int SellableRoomCount,
    int ActiveReservationCount,
    int AvailableRoomCount,
    bool HasAvailability);
