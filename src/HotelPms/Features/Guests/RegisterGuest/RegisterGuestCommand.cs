using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Guests.RegisterGuest;

public sealed record RegisterGuestCommand(
    TenantId TenantId,
    string Name,
    string? Email,
    string? PhoneNumber);
