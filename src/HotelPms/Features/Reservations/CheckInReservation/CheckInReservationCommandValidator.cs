using FluentValidation;

namespace HotelPms.Features.Reservations.CheckInReservation;

public sealed class CheckInReservationCommandValidator : AbstractValidator<CheckInReservationCommand>
{
    public CheckInReservationCommandValidator()
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(command => command.ReservationId.Value)
            .NotEmpty()
            .WithMessage("Reservation ID must have value.");

        RuleFor(command => command.RoomId.Value)
            .NotEmpty()
            .WithMessage("Room ID must have value.");
    }
}
