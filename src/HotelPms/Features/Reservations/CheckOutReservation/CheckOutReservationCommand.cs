using HotelPms.Features.Reservations.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.CheckOutReservation;

public sealed record CheckOutReservationCommand(
    TenantId TenantId,
    ReservationId ReservationId);
