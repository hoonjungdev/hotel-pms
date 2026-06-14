using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Housekeeping.MarkRoomClean;

public sealed record MarkRoomCleanCommand(TenantId TenantId, RoomId RoomId);
