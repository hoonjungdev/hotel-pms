using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.CheckReservationAvailability;

public sealed record CheckReservationAvailabilityQuery(
    TenantId TenantId,
    RoomTypeId RoomTypeId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate);
