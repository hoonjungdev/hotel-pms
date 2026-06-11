using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.AddRoom;

public sealed record AddRoomCommand(TenantId TenantId, RoomTypeId RoomTypeId, string Number);
