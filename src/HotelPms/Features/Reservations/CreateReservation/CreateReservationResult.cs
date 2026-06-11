using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.Reservations.CreateReservation;

public sealed record CreateReservationResult(
    ReservationId Id,
    GuestId PrimaryGuestId,
    RoomTypeId RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount,
    string Status);
