using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.UnitTests.Features.Shared.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Zero_ReturnsZeroAmountWithGivenCurrency()
    {
        Money m1 = Money.Zero(Currency.USD);
        
        Assert.Equal(0, m1.Amount);
    }
    
    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        // Arrange
        Money m1 = new Money(10, Currency.USD);
        Money m2 = new Money(20, Currency.USD);

        // Act
        Money sum = m1 + m2;

        // Assert
        Assert.Equal(30, sum.Amount);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsException()
    {
        Money krw =  new Money(10_000, Currency.KRW);
        Money usd =  new Money(20, Currency.USD);
        
        Assert.Throws<InvalidOperationException>(() => krw + usd);
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        Money m1 = new Money(20, Currency.USD);
        Money m2 = new Money(10, Currency.USD);
        
        Money subtract = m1 - m2;
        
        Assert.Equal(10, subtract.Amount);
    }

    [Fact]
    public void Subtract_ResultingInNegative_ThrowsException()
    {
        Money m1 = new Money(10, Currency.USD);
        Money m2 = new Money(20, Currency.USD);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => m1 - m2);
    }

    [Fact]
    public void Subtract_DifferentCurrency_ThrowsException()
    {
        Money krw = new Money(10_000, Currency.KRW);
        Money usd = new Money(20, Currency.USD);
        
        Assert.Throws<InvalidOperationException>(() => krw - usd);
    }

    [Fact]
    public void Multiply_ByScalar_ReturnsScaledAmount()
    {
        Money m1 = new Money(20, Currency.USD);

        Money result = m1 * 3;
        
        Assert.Equal(60, result.Amount);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-10, Currency.USD));
        
        Assert.Equal("amount", ex.ParamName);
    }
    
    [Fact]
    public void ToString_KrwCurrency_ReturnsWonFormat()
    {
        Money m1 = new Money(20, Currency.KRW);
        
        string result = m1.ToString();
        
        Assert.Equal("₩20", result);
    }
    
    [Fact]
    public void ToString_UsdCurrency_ReturnsUsdFormat()
    {
        Money m1 = new Money(20, Currency.USD);
        
        string result = m1.ToString();
        
        Assert.Equal("$20.00", result);
    }
    
    [Fact]
    public void ToString_JpyCurrency_ReturnsYenFormat()
    {
        Money m1 = new Money(20, Currency.JPY);
        
        string result = m1.ToString();
        
        Assert.Equal("¥20", result);
    }
}