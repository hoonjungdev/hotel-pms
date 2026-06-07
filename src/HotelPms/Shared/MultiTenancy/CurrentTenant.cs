namespace HotelPms.Shared.MultiTenancy;

public sealed class CurrentTenant
{
    public TenantId Id { get; } = WorkspaceTenant.Id;
}
