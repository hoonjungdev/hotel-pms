namespace HotelPms.Features.RoomTypes.Domain.ValueObjects;

public sealed record RoomTypeCode
{
    public string Value { get; }

    private RoomTypeCode(string value)
    {
        Value = value;
    }

    public static RoomTypeCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Room type code must be provided.", nameof(value));
        }

        string normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > 20)
        {
            throw new ArgumentException("Room type code is too long.", nameof(value));
        }

        return new RoomTypeCode(normalized);
    }

    public override string ToString() => Value;
}
