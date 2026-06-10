using FluentValidation;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.RoomTypes.CreateRoomType;

internal static class CreateRoomTypeEndpoint
{
    public static RouteGroupBuilder MapCreateRoomTypeEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateRoomTypeAsync)
            .WithName("CreateRoomType");

        return group;
    }

    private static async Task<Results<Created<RoomTypeResponse>, ValidationProblem>> CreateRoomTypeAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        CreateRoomTypeRequest request,
        CreateRoomTypeHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomTypeCommand(
            new TenantId(tenantId),
            request.Code,
            request.Name,
            request.BaseOccupancy,
            request.MaxOccupancy);

        try
        {
            CreateRoomTypeResult result = await handler.HandleAsync(command, cancellationToken);

            return TypedResults.Created($"/api/room-types/{result.Id.Value}", ToResponse(result));
        }
        catch (ValidationException exception)
        {
            return TypedResults.ValidationProblem(ValidationProblemMapper.ToErrors(exception, "RoomType"));
        }
    }

    private static RoomTypeResponse ToResponse(CreateRoomTypeResult result)
    {
        return new RoomTypeResponse(
            result.Id.Value,
            result.Code,
            result.Name,
            result.BaseOccupancy,
            result.MaxOccupancy);
    }
}
