using HotelPms.Features.Guests.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Guests.GetGuest;

internal static class GetGuestEndpoint
{
    public static RouteGroupBuilder MapGetGuestEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{guestId:guid}", GetGuestAsync)
            .WithName("GetGuest");

        return group;
    }

    private static async Task<Results<Ok<GuestResponse>, NotFound>> GetGuestAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid guestId,
        GetGuestHandler handler,
        CancellationToken cancellationToken)
    {
        GuestDetails? guest = await handler.HandleAsync(
            new GetGuestQuery(new TenantId(tenantId), new GuestId(guestId)),
            cancellationToken);

        return guest is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ToResponse(guest));
    }

    internal static GuestResponse ToResponse(GuestDetails guest)
    {
        return new GuestResponse(
            guest.Id.Value,
            guest.Name,
            guest.Email,
            guest.PhoneNumber);
    }
}
