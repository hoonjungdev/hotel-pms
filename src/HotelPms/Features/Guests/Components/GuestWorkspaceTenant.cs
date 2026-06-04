using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Guests.Components;

public static class GuestWorkspaceTenant
{
    public static readonly TenantId Id = new(Guid.Parse("f2d66c5a-0f23-44d4-8f7a-63ea5bfe29f0"));
}
