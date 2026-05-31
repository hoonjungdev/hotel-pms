using HotelPms.Shared.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Guests.Domain;

public sealed record GuestRegistered(GuestId GuestId, TenantId TenantId) : IDomainEvent;
