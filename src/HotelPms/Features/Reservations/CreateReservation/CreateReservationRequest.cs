namespace HotelPms.Features.Reservations.CreateReservation;

public sealed record CreateReservationRequest(
    Guid PrimaryGuestId,
    Guid RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount);
