using FluentValidation;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Rooms.AddRoom;

internal static class AddRoomEndpoint
{
    public static RouteGroupBuilder MapAddRoomEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", AddRoomAsync)
            .WithName("AddRoom");

        return group;
    }

    private static async Task<Results<Created<RoomResponse>, ValidationProblem>> AddRoomAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        AddRoomRequest request,
        AddRoomHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new AddRoomCommand(new TenantId(tenantId), request.Number);

        try
        {
            AddRoomResult result = await handler.HandleAsync(command, cancellationToken);

            return TypedResults.Created($"/api/rooms/{result.Id.Value}", ToResponse(result));
        }
        catch (ValidationException exception)
        {
            return TypedResults.ValidationProblem(ValidationProblemMapper.ToErrors(exception, "Room"));
        }
    }

    private static RoomResponse ToResponse(AddRoomResult result)
    {
        return new RoomResponse(
            result.Id.Value,
            result.Number,
            result.Condition);
    }
}
