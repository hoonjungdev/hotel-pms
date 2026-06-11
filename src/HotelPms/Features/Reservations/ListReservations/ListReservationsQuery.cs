using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Reservations.ListReservations;

public sealed record ListReservationsQuery(TenantId TenantId);
