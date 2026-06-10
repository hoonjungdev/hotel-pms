using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.RoomTypes.ListRoomTypes;

public sealed record ListRoomTypesQuery(TenantId TenantId);
