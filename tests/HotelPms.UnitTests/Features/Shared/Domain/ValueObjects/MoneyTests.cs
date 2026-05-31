using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.UnitTests.Features.Shared.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Zero_ReturnsZeroAmountWithGivenCurrency()
    {
        var m1 = Money.Zero(Currency.USD);

        Assert.Equal(0, m1.Amount);
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        // Arrange
        var m1 = new Money(10, Currency.USD);
        var m2 = new Money(20, Currency.USD);

        // Act
        Money sum = m1 + m2;

        // Assert
        Assert.Equal(30, sum.Amount);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsException()
    {
        var krw = new Money(10_000, Currency.KRW);
        var usd = new Money(20, Currency.USD);

        Assert.Throws<InvalidOperationException>(() => krw + usd);
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        var m1 = new Money(20, Currency.USD);
        var m2 = new Money(10, Currency.USD);

        Money subtract = m1 - m2;

        Assert.Equal(10, subtract.Amount);
    }

    [Fact]
    public void Subtract_ResultingInNegative_ThrowsException()
    {
        var m1 = new Money(10, Currency.USD);
        var m2 = new Money(20, Currency.USD);

        Assert.Throws<ArgumentOutOfRangeException>(() => m1 - m2);
    }

    [Fact]
    public void Subtract_DifferentCurrency_ThrowsException()
    {
        var krw = new Money(10_000, Currency.KRW);
        var usd = new Money(20, Currency.USD);

        Assert.Throws<InvalidOperationException>(() => krw - usd);
    }

    [Fact]
    public void Multiply_ByScalar_ReturnsScaledAmount()
    {
        var m1 = new Money(20, Currency.USD);

        Money result = m1 * 3;

        Assert.Equal(60, result.Amount);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsException()
    {
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-10, Currency.USD));

        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void ToString_KrwCurrency_ReturnsWonFormat()
    {
        var m1 = new Money(20, Currency.KRW);

        string result = m1.ToString();

        Assert.Equal("₩20", result);
    }

    [Fact]
    public void ToString_UsdCurrency_ReturnsUsdFormat()
    {
        var m1 = new Money(20, Currency.USD);

        string result = m1.ToString();

        Assert.Equal("$20.00", result);
    }

    [Fact]
    public void ToString_JpyCurrency_ReturnsYenFormat()
    {
        var m1 = new Money(20, Currency.JPY);

        string result = m1.ToString();

        Assert.Equal("¥20", result);
    }
}
