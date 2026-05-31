namespace HotelPms.Features.Guests.Domain.ValueObjects;

public sealed record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone number cannot be empty.", nameof(value));
        }

        string trimmed = value.Trim();

        if (trimmed.IndexOf('+', 1) > 0)
        {
            throw new ArgumentException($"Invalid phone number format: {value}", nameof(value));
        }

        bool hasLeadingPlus = trimmed.StartsWith('+');
        string digits = new string(trimmed.Where(char.IsDigit).ToArray());
        string normalized = hasLeadingPlus ? "+" + digits : digits;

        if (!IsValidFormat(normalized))
        {
            throw new ArgumentException($"Invalid phone number format: {value}", nameof(value));
        }

        return new PhoneNumber(normalized);
    }

    private static bool IsValidFormat(string normalized)
    {
        int digitCount = normalized.Count(char.IsDigit);
        return digitCount is >= 7 and <= 15;
    }
}
