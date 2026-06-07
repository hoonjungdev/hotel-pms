using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.Application;

public sealed record UpdateRoomConditionCommand(TenantId TenantId, RoomId RoomId, RoomCondition Condition);
