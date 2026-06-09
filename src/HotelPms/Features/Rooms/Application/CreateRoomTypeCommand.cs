using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.Application;

public sealed record CreateRoomTypeCommand(
    TenantId TenantId,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy);
