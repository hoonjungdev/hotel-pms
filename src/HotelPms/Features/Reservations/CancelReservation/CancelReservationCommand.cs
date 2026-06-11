using HotelPms.Features.Reservations.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.CancelReservation;

public sealed record CancelReservationCommand(TenantId TenantId, ReservationId ReservationId);
