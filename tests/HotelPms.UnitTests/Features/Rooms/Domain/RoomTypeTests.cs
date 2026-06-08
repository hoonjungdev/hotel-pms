using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.UnitTests.Features.Rooms.Domain;

public class RoomTypeTests
{
    [Fact]
    public void Create_ValidRoomType_ReturnsRoomType()
    {
        var tenantId = TenantId.New();
        var code = RoomTypeCode.Create("dbl");

        var roomType = RoomType.Create(tenantId, code, "Double", 2, 4);

        Assert.Equal(tenantId, roomType.TenantId);
        Assert.Equal(code, roomType.Code);
        Assert.Equal("Double", roomType.Name);
        Assert.Equal(2, roomType.BaseOccupancy);
        Assert.Equal(4, roomType.MaxOccupancy);
    }

    [Fact]
    public void Create_NameWithWhitespace_ReturnsTrimmedName()
    {
        var roomType = RoomType.Create(TenantId.New(), RoomTypeCode.Create("dbl"), "  Double  ", 2, 4);

        Assert.Equal("Double", roomType.Name);
    }

    [Fact]
    public void Create_EmptyTenantId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            RoomType.Create(new TenantId(Guid.Empty), RoomTypeCode.Create("dbl"), "Double", 2, 4));
    }

    [Fact]
    public void Create_NullCode_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            RoomType.Create(TenantId.New(), null!, "Double", 2, 4));
    }

    [Fact]
    public void Create_EmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            RoomType.Create(TenantId.New(), RoomTypeCode.Create("dbl"), "", 2, 4));
    }

    [Fact]
    public void Create_NullName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            RoomType.Create(TenantId.New(), RoomTypeCode.Create("dbl"), null!, 2, 4));
    }

    [Fact]
    public void Create_TooLongName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            RoomType.Create(TenantId.New(), RoomTypeCode.Create("dbl"), new string('a', 101), 2, 4));
    }

    [Fact]
    public void Create_BaseOccupancyLessThanOne_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            RoomType.Create(TenantId.New(), RoomTypeCode.Create("dbl"), "Double", 0, 4));
    }

    [Fact]
    public void Create_MaxOccupancyLessThanBaseOccupancy_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            RoomType.Create(TenantId.New(), RoomTypeCode.Create("dbl"), "Double", 4, 2));
    }
}
