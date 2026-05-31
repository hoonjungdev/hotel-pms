using HotelPms.Features.Guests.Domain;

namespace HotelPms.UnitTests.Features.Guests.Domain;

public class GuestIdTests
{
    [Fact]
    public void New_CalledTwice_ReturnsDifferentIds()
    {
        // Arrange
        // Act
        var first = GuestId.New();
        var second = GuestId.New();

        // Assert
        Assert.NotEqual(first, second);
    }
}
