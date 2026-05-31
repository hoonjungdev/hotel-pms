namespace HotelPms.Shared.Domain.ValueObjects;

public readonly record struct DateRange
{
    public DateOnly Start { get; }
    public DateOnly End { get; }
    public int Nights => End.DayNumber - Start.DayNumber;

    public DateRange(DateOnly start, DateOnly end)
    {
        if (start >= end)
        {
            throw new ArgumentException("End date must be after start date.", nameof(end));
        }

        Start = start;
        End = end;
    }

    public bool Overlaps(DateRange other) => Start < other.End && other.Start < End;

    public override string ToString()
    {
        return $"{Start:yyyy-MM-dd} ~ {End:yyyy-MM-dd} ({Nights} {(Nights > 1 ? "nights" : "night")})";
    }
}
