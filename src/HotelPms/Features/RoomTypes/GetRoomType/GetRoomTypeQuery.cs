using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.RoomTypes.GetRoomType;

public sealed record GetRoomTypeQuery(TenantId TenantId, RoomTypeId RoomTypeId);
