using HotelPms.Features.Guests.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.Domain;

public sealed record ReservationCreated(
    ReservationId ReservationId,
    TenantId TenantId,
    GuestId PrimaryGuestId,
    RoomTypeId RoomTypeId) : IDomainEvent;
