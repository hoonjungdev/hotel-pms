using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.Reservations.ConfirmReservation;

public sealed record ConfirmReservationResult(
    ReservationId Id,
    GuestId PrimaryGuestId,
    RoomTypeId RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount,
    Money TotalAmount,
    string Status);
