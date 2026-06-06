namespace HotelPms.Features.Rooms.Domain.ValueObjects;

public sealed record RoomNumber
{
    public string Value { get; }

    private RoomNumber(string value)
    {
        Value = value;
    }

    public static RoomNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A room number must be provided.", nameof(value));
        }

        string normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > 20)
        {
            throw new ArgumentException("A room number is too long.", nameof(value));
        }

        return new RoomNumber(normalized);
    }

    public override string ToString() => Value;
}
