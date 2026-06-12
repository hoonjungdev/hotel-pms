namespace HotelPms.Features.Reservations;

public sealed record ReservationResponse(
    Guid Id,
    Guid PrimaryGuestId,
    Guid RoomTypeId,
    Guid? AssignedRoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount,
    decimal TotalAmount,
    string TotalCurrency,
    string Status);
