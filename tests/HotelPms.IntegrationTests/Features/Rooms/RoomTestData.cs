using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Rooms;

internal static class RoomTestData
{
    public static RoomType CreateRoomType(TenantId tenantId)
    {
        string code = $"RT{Guid.NewGuid():N}"[..20];

        return RoomType.Create(
            tenantId,
            RoomTypeCode.Create(code),
            "Double",
            baseOccupancy: 2,
            maxOccupancy: 3);
    }

    public static Room CreateRoom(TenantId tenantId, RoomType roomType, string number)
    {
        return Room.Create(tenantId, roomType.Id, RoomNumber.Create(number));
    }
}
