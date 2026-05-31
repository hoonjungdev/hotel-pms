using System.Globalization;

namespace HotelPms.Shared.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative");
        }

        Amount = amount;
        Currency = currency;
    }

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Currency mismatch. {left.Currency} != {right.Currency}");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Currency mismatch. {left.Currency} != {right.Currency}");
        }

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    public static Money Zero(Currency currency)
    {
        return new Money(0, currency);
    }

    public override string ToString()
    {
        CultureInfo culture = Currency switch
        {
            Currency.KRW => CultureInfo.CreateSpecificCulture("ko-KR"),
            Currency.USD => CultureInfo.CreateSpecificCulture("en-US"),
            Currency.JPY => CultureInfo.CreateSpecificCulture("ja-JP"),
            _ => throw new ArgumentException("Unknown currency")
        };

        return Amount.ToString("C", culture);
    }
}
