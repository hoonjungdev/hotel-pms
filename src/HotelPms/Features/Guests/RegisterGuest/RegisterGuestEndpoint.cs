using FluentValidation;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Guests.RegisterGuest;

internal static class RegisterGuestEndpoint
{
    public static RouteGroupBuilder MapRegisterGuestEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", RegisterGuestAsync)
            .WithName("RegisterGuest");

        return group;
    }

    private static async Task<Results<Created<GuestResponse>, ValidationProblem>> RegisterGuestAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        RegisterGuestRequest request,
        RegisterGuestHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterGuestCommand(
            new TenantId(tenantId),
            request.Name,
            request.Email,
            request.PhoneNumber);

        try
        {
            RegisterGuestResult result = await handler.HandleAsync(command, cancellationToken);

            return TypedResults.Created($"/api/guests/{result.Id.Value}", ToResponse(result));
        }
        catch (ValidationException exception)
        {
            return TypedResults.ValidationProblem(ValidationProblemMapper.ToErrors(exception, "Guest"));
        }
    }

    private static GuestResponse ToResponse(RegisterGuestResult result)
    {
        return new GuestResponse(
            result.Id.Value,
            result.Name,
            result.Email,
            result.PhoneNumber);
    }
}
