using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.Reservations.ListReservations;

public sealed record ReservationListItem(
    ReservationId Id,
    GuestId PrimaryGuestId,
    RoomTypeId RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount,
    Money TotalAmount,
    string Status);
