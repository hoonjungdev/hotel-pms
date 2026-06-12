using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.Reservations.CancelReservation;

public sealed record CancelReservationResult(
    ReservationId Id,
    GuestId PrimaryGuestId,
    RoomTypeId RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount,
    Money TotalAmount,
    string Status);
