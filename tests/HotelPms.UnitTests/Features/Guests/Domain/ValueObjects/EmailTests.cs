using HotelPms.Features.Guests.Domain.ValueObjects;

namespace HotelPms.UnitTests.Features.Guests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsException(string? value)
    {
        Assert.Throws<ArgumentException>(() => Email.Create(value));
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("missing-dot@domain")]
    [InlineData("@no-local.com")]
    [InlineData("abc@no-local.")]
    public void Create_InvalidFormat_ThrowsException(string invalidEmail)
    {
        Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail));
    }

    [Theory]
    [InlineData(" Valid@format.email", "valid@format.email")]
    [InlineData("First.Last@Example.com", "first.last@example.com")]
    public void Create_ValidFormat_ReturnsNormalizedEmail(string input, string expected)
    {
        var email = Email.Create(input);

        Assert.Equal(expected, email.Value);
    }

    [Fact]
    public void Create_TooLong_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Email.Create("abopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrst@no-local.com"));
    }
}
