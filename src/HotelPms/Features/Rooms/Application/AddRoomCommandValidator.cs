using FluentValidation;

namespace HotelPms.Features.Rooms.Application;

public sealed class AddRoomCommandValidator : AbstractValidator<AddRoomCommand>
{
    public AddRoomCommandValidator()
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(command => command.Number)
            .NotEmpty()
            .WithMessage("A room number must be provided.")
            .MaximumLength(20)
            .WithMessage("A room number is too long.");
    }
}
