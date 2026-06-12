using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.CheckInReservation;

public sealed record CheckInReservationCommand(
    TenantId TenantId,
    ReservationId ReservationId,
    RoomId RoomId);
