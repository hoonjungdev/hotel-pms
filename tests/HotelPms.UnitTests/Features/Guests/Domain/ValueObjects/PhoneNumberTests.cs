using HotelPms.Features.Guests.Domain.ValueObjects;

namespace HotelPms.UnitTests.Features.Guests.Domain.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsException(string? value)
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Create(value));
    }

    [Theory]
    [InlineData(" +10")]
    [InlineData("+1012345678912345 ")]
    [InlineData("10")]
    [InlineData("+1012341+234")]
    public void Create_InvalidFormat_ThrowsException(string invalidPhoneNumber)
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Create(invalidPhoneNumber));
    }

    [Theory]
    [InlineData("010-1234-5678", "01012345678")]
    [InlineData("(010) 1234.5678", "01012345678")]
    public void Create_FormattingCharacters_NormalizesToDigits(string input, string expected)
    {
        Assert.Equal(expected, PhoneNumber.Create(input).Value);
    }

    [Fact]
    public void Create_SameNumberDifferentFormatting_ProducesEqualValues()
    {
        Assert.Equal(PhoneNumber.Create("010-1234-5678"), PhoneNumber.Create("010 1234 5678"));
    }

    [Fact]
    public void Create_LeadingPlus_PreservesCountryCode()
    {
        Assert.Equal("+821012345678", PhoneNumber.Create("+8210-1234-5678").Value);
    }

    [Fact]
    public void Create_DomesticNumberWithoutPlus_ReturnsPhoneNumber()
    {
        Assert.Equal("01012345678", PhoneNumber.Create("010-1234-5678").Value);
    }
}
