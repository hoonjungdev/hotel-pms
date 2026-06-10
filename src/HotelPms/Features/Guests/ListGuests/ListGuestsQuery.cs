using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Guests.ListGuests;

public sealed record ListGuestsQuery(TenantId TenantId);
