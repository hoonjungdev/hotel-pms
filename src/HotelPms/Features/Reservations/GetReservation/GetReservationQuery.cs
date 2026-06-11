using HotelPms.Features.Reservations.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.GetReservation;

public sealed record GetReservationQuery(TenantId TenantId, ReservationId ReservationId);
