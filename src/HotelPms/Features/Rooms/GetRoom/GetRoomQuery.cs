using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.GetRoom;

public sealed record GetRoomQuery(TenantId TenantId, RoomId RoomId);
