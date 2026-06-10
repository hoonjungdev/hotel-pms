using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.UnitTests.Features.RoomTypes.Domain;

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
