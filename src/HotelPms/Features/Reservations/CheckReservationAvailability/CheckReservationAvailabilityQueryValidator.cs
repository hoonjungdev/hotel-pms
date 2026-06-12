using FluentValidation;

namespace HotelPms.Features.Reservations.CheckReservationAvailability;

public sealed class CheckReservationAvailabilityQueryValidator
    : AbstractValidator<CheckReservationAvailabilityQuery>
{
    public CheckReservationAvailabilityQueryValidator()
    {
        RuleFor(query => query.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(query => query.RoomTypeId.Value)
            .NotEmpty()
            .WithMessage("Room type ID must have value.");

        RuleFor(query => query.CheckOutDate)
            .GreaterThan(query => query.CheckInDate)
            .WithMessage("Check-out date must be after check-in date.");
    }
}
