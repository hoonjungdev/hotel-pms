using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelPms.Infrastructure.Database.Converters;

public sealed class TenantIdConverter : ValueConverter<TenantId, Guid>
{
    public TenantIdConverter() : base(tenantId => tenantId.Value, tenantId => new TenantId(tenantId))
    {
    }
}
