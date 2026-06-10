using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.AddRoom;

public sealed record AddRoomCommand(TenantId TenantId, string Number);
