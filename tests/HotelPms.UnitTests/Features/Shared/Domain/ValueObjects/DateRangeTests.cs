using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.UnitTests.Features.Shared.Domain.ValueObjects;

public class DateRangeTests
{
    [Fact]
    public void Constructor_EndBeforeStart_ThrowsArgumentException()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 9);

        Assert.Throws<ArgumentException>(() => new DateRange(start, end));
    }
    
    [Fact]
    public void Constructor_EndEqualsStart_ThrowsArgumentException()
    {
        var start = new DateOnly(2026, 6, 10);

        Assert.Throws<ArgumentException>(() => new DateRange(start, start));
    }
    
    [Fact]
    public void Nights_TwoNightStay_ReturnsTwo()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 12);
        
        var dateRange = new DateRange(start, end);
        
        Assert.Equal(2, dateRange.Nights);
    }

    [Fact]
    public void Overlaps_AdjacentRanges_ReturnsFalse()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 11);
        
        var dateRange = new DateRange(start, end);
        
        var otherStart = new DateOnly(2026, 6, 11);
        var otherEnd = new DateOnly(2026, 6, 12);
        
        var otherRange = new DateRange(otherStart, otherEnd);
        
        Assert.False(dateRange.Overlaps(otherRange));
    }

    [Fact]
    public void Overlaps_PartialOverlap_ReturnsTrue()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 13);
        
        var dateRange = new DateRange(start, end);
        
        var otherStart = new DateOnly(2026, 6, 12);
        var otherEnd = new DateOnly(2026, 6, 14);
        
        var otherRange = new DateRange(otherStart, otherEnd);
        
        Assert.True(dateRange.Overlaps(otherRange));
    }

    [Fact]
    public void Overlaps_OneContainsOther_ReturnsTrue()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 15);
        
        var dateRange = new DateRange(start, end);
        
        var otherStart = new DateOnly(2026, 6, 11);
        var otherEnd = new DateOnly(2026, 6, 12);
        
        var otherRange = new DateRange(otherStart, otherEnd);
        
        Assert.True(dateRange.Overlaps(otherRange));
    }

    [Fact]
    public void Overlaps_Disjoint_ReturnsFalse()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 15);
        
        var dateRange = new DateRange(start, end);
        
        var otherStart = new DateOnly(2026, 7, 11);
        var otherEnd = new DateOnly(2026, 7, 12);
        
        var otherRange = new DateRange(otherStart, otherEnd);
        
        Assert.False(dateRange.Overlaps(otherRange));
    }

    [Fact]
    public void Overlaps_Identical_ReturnsTrue()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 15);
        
        var dateRange = new DateRange(start, end);
        
        Assert.True(dateRange.Overlaps(dateRange));
    }

    [Fact]
    public void Overlaps_OperandOrderSwapped_ReturnsTrueBothWays()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 15);
        
        var dateRange = new DateRange(start, end);
        
        var otherStart = new DateOnly(2026, 6, 11);
        var otherEnd = new DateOnly(2026, 6, 16);
        
        var otherRange = new DateRange(otherStart, otherEnd);

        var forward = dateRange.Overlaps(otherRange);
        var reverse = otherRange.Overlaps(dateRange);

        Assert.True(forward);
        Assert.True(reverse);
    }

    [Fact]
    public void ToString_OneNight_ReturnsNight()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 11);
        
        var dateRange = new DateRange(start, end);
        
        Assert.Equal("2026-06-10 ~ 2026-06-11 (1 night)", dateRange.ToString());
    }

    [Fact]
    public void ToString_OverOneNight_ReturnsNights()
    {
        var start = new DateOnly(2026, 6, 10);
        var end = new DateOnly(2026, 6, 13);
        
        var dateRange = new DateRange(start, end);
        
        Assert.Equal("2026-06-10 ~ 2026-06-13 (3 nights)", dateRange.ToString());
    }
}