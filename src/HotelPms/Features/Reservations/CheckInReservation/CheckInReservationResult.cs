using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.Reservations.CheckInReservation;

public sealed record CheckInReservationResult(
    ReservationId Id,
    GuestId PrimaryGuestId,
    RoomTypeId RoomTypeId,
    RoomId? AssignedRoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount,
    Money TotalAmount,
    string Status);
