using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.ListRooms;

public sealed record ListRoomsQuery(TenantId TenantId);
