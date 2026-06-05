using HotelPms.Features.Guests.Domain;

namespace HotelPms.Features.Guests.Application;

public sealed record GuestDetails(GuestId Id, string Name, string? Email, string? PhoneNumber);
