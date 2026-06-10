namespace HotelPms.Features.Guests.RegisterGuest;

public sealed record RegisterGuestRequest(
    string Name,
    string? Email,
    string? PhoneNumber);
