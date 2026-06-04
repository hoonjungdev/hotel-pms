using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Guests.Application;

public sealed record ListGuestsQuery(TenantId TenantId);
