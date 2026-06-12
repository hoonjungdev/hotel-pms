using HotelPms.Features.Pricing.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.UnitTests.Features.Pricing.Domain;

public class PriceCalculatorTests
{
    [Fact]
    public void CalculateStayTotal_TwoNightStay_ReturnsNightlyRateMultipliedByNights()
    {
        var nightlyRate = new Money(120_000, Currency.KRW);
        var stayPeriod = new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3));

        Money total = PriceCalculator.CalculateStayTotal(nightlyRate, stayPeriod);

        Assert.Equal(new Money(240_000, Currency.KRW), total);
    }

    [Fact]
    public void CalculateStayTotal_OneNightStay_ReturnsNightlyRate()
    {
        var nightlyRate = new Money(120_000, Currency.KRW);
        var stayPeriod = new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 2));

        Money total = PriceCalculator.CalculateStayTotal(nightlyRate, stayPeriod);

        Assert.Equal(nightlyRate, total);
    }
}
