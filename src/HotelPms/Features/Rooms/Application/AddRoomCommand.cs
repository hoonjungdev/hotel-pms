using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.Application;

public sealed record AddRoomCommand(TenantId TenantId, string Number);
