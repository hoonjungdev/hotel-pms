using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.RoomTypes.CreateRoomType;

public sealed record CreateRoomTypeCommand(
    TenantId TenantId,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy,
    decimal BaseNightlyRateAmount,
    string BaseNightlyRateCurrency);
