using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Housekeeping.ListHousekeepingRooms;

public sealed record ListHousekeepingRoomsQuery(TenantId TenantId, RoomCondition? Condition);
