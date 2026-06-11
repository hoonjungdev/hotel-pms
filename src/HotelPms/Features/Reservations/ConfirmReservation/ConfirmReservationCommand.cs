using HotelPms.Features.Reservations.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.ConfirmReservation;

public sealed record ConfirmReservationCommand(TenantId TenantId, ReservationId ReservationId);
