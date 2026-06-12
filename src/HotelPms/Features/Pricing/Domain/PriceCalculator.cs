using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.Pricing.Domain;

public static class PriceCalculator
{
    public static Money CalculateStayTotal(Money nightlyRate, DateRange stayPeriod)
    {
        return nightlyRate * stayPeriod.Nights;
    }
}
