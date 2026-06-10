using HotelPms.Features.Guests.Domain;

namespace HotelPms.Features.Guests.RegisterGuest;

public sealed record RegisterGuestResult(GuestId Id, string Name, string? Email, string? PhoneNumber);
