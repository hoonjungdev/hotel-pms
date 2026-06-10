using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Guests.ListGuests;

internal static class ListGuestsEndpoint
{
    public static RouteGroupBuilder MapListGuestsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListGuestsAsync)
            .WithName("ListGuests");

        return group;
    }

    private static async Task<Ok<IReadOnlyList<GuestResponse>>> ListGuestsAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        ListGuestsHandler handler,
        CancellationToken cancellationToken)
    {
        List<GuestListItem> guests = await handler.HandleAsync(
            new ListGuestsQuery(new TenantId(tenantId)),
            cancellationToken);

        IReadOnlyList<GuestResponse> response = guests
            .Select(ToResponse)
            .ToList();

        return TypedResults.Ok(response);
    }

    private static GuestResponse ToResponse(GuestListItem guest)
    {
        return new GuestResponse(
            guest.Id.Value,
            guest.Name,
            guest.Email,
            guest.PhoneNumber);
    }
}
