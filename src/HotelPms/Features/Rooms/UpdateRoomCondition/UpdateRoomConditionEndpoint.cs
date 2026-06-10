using FluentValidation;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Shared.Api;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HotelPms.Features.Rooms.UpdateRoomCondition;

internal static class UpdateRoomConditionEndpoint
{
    public static RouteGroupBuilder MapUpdateRoomConditionEndpoint(this RouteGroupBuilder group)
    {
        group.MapPatch("/{roomId:guid}/condition", UpdateRoomConditionAsync)
            .WithName("UpdateRoomCondition");

        return group;
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> UpdateRoomConditionAsync(
        [FromHeader(Name = ApiDefaults.TenantHeaderName)] Guid tenantId,
        Guid roomId,
        UpdateRoomConditionRequest request,
        UpdateRoomConditionHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.Condition, ignoreCase: true, out RoomCondition condition))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Condition)] = ["Unsupported room condition."]
            });
        }

        try
        {
            UpdateRoomConditionResult? result = await handler.HandleAsync(
                new UpdateRoomConditionCommand(new TenantId(tenantId), new RoomId(roomId), condition),
                cancellationToken);

            return result is not null
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        }
        catch (ValidationException exception)
        {
            return TypedResults.ValidationProblem(ValidationProblemMapper.ToErrors(exception, "Room"));
        }
    }
}
