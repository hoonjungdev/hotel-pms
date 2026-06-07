using FluentValidation;

namespace HotelPms.Features.Rooms.Application;

public sealed class UpdateRoomConditionCommandValidator : AbstractValidator<UpdateRoomConditionCommand>
{
    public UpdateRoomConditionCommandValidator()
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(command => command.RoomId.Value)
            .NotEmpty()
            .WithMessage("Room ID must have value.");
    }
}
