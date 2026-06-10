namespace HotelPms.Features.Guests;

public sealed record GuestResponse(
    Guid Id,
    string Name,
    string? Email,
    string? PhoneNumber);
