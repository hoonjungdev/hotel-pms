using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.Application;

public sealed record ListRoomsQuery(TenantId TenantId);
