using HotelPms.Features.Guests.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.CreateReservation;

public sealed record CreateReservationCommand(
    TenantId TenantId,
    GuestId PrimaryGuestId,
    RoomTypeId RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int GuestCount);
