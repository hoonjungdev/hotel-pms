using HotelPms.Features.RoomTypes.Domain.ValueObjects;

namespace HotelPms.UnitTests.Features.RoomTypes.Domain.ValueObjects;

public class RoomTypeCodeTests
{
    [Fact]
    public void Create_LowercaseCode_ReturnsUppercaseCode()
    {
        var code = RoomTypeCode.Create("dbl");

        Assert.Equal("DBL", code.Value);
    }

    [Fact]
    public void Create_CodeWithWhitespace_ReturnsTrimmedCode()
    {
        var code = RoomTypeCode.Create("  fam  ");

        Assert.Equal("FAM", code.Value);
    }

    [Fact]
    public void Create_EmptyCode_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => RoomTypeCode.Create(""));
    }

    [Fact]
    public void Create_TooLongCode_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => RoomTypeCode.Create("ROOMTYPECODEISTOOLONG"));
    }

    [Fact]
    public void ToString_Always_ReturnsValue()
    {
        var code = RoomTypeCode.Create("std");

        Assert.Equal("STD", code.ToString());
    }
}
