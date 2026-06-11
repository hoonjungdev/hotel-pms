using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.UnitTests.Features.Rooms.Domain;

public class RoomTests
{
    [Fact]
    public void Create_ValidRoom_ReturnsCleanRoom()
    {
        var tenantId = TenantId.New();
        var roomTypeId = RoomTypeId.New();

        var room = Room.Create(tenantId, roomTypeId, RoomNumber.Create(" a101 "));

        Assert.Equal(tenantId, room.TenantId);
        Assert.Equal(roomTypeId, room.RoomTypeId);
        Assert.Equal("A101", room.Number.Value);
        Assert.Equal(RoomCondition.Clean, room.Condition);
    }

    [Fact]
    public void Create_EmptyRoomTypeId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            Room.Create(
                TenantId.New(),
                new RoomTypeId(Guid.Empty),
                RoomNumber.Create("A101")));
    }
}
