using HotelPms.Features.Guests.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Guests.Application;

public sealed record GetGuestQuery(TenantId TenantId, GuestId GuestId);
