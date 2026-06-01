namespace HotelPms.Features.Guests.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty.");
        }

        string normalized = value.Trim().ToLowerInvariant();

        if (!IsValidFormat(normalized))
        {
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));
        }

        return new Email(normalized);
    }

    private static bool IsValidFormat(string value)
    {
        if (value.EndsWith('.') || value.EndsWith('@') || value.StartsWith('@') || value.Length > 254)
        {
            return false;
        }

        string[] parts = value.Split('@');
        if (parts.Length != 2)
        {
            return false;
        }

        string domain = parts[1];
        int indexOfDot = domain.IndexOf('.');

        return indexOfDot >= 0;
    }
}
