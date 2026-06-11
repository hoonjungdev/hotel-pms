namespace HotelPms.Features.Reservations;

public sealed record ReservationResponse(
    Guid Id,
    Guid PrimaryGuestId,
    Guid RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount,
    string Status);
