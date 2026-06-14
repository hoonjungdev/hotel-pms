using FluentValidation;

namespace HotelPms.Features.Housekeeping.MarkRoomClean;

public sealed class MarkRoomCleanCommandValidator : AbstractValidator<MarkRoomCleanCommand>
{
    public MarkRoomCleanCommandValidator()
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(command => command.RoomId.Value)
            .NotEmpty()
            .WithMessage("Room ID must have value.");
    }
}
