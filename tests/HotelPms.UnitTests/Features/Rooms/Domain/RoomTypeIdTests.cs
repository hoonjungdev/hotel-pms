using HotelPms.Features.Rooms.Domain;

namespace HotelPms.UnitTests.Features.Rooms.Domain;

public class RoomTypeIdTests
{
    [Fact]
    public void New_CalledTwice_ReturnsDifferentIds()
    {
        var first = RoomTypeId.New();
        var second = RoomTypeId.New();

        Assert.NotEqual(first, second);
    }
}
