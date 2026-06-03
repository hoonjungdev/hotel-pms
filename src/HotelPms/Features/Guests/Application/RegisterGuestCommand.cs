using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Guests.Application;

public sealed record RegisterGuestCommand(
    TenantId TenantId,
    string Name,
    string? Email,
    string? PhoneNumber);
