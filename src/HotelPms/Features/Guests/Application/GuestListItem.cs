using HotelPms.Features.Guests.Domain;

namespace HotelPms.Features.Guests.Application;

public sealed record GuestListItem(GuestId Id, string Name, string? Email, string? PhoneNumber);
