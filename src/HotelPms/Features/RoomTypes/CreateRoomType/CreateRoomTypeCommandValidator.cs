using FluentValidation;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.RoomTypes.CreateRoomType;

public sealed class CreateRoomTypeCommandValidator : AbstractValidator<CreateRoomTypeCommand>
{
    public CreateRoomTypeCommandValidator()
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(command => command.Code)
            .NotEmpty()
            .WithMessage("A room type code must be provided.")
            .MaximumLength(20)
            .WithMessage("A room type code is too long.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("A room type name must be provided.")
            .MaximumLength(100)
            .WithMessage("A room type name is too long.");

        RuleFor(command => command.BaseOccupancy)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Base occupancy must be at least 1.");

        RuleFor(command => command.MaxOccupancy)
            .GreaterThanOrEqualTo(command => command.BaseOccupancy)
            .WithMessage("Max occupancy must be greater than or equal to base occupancy.");

        RuleFor(command => command.BaseNightlyRateAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Base nightly rate amount cannot be negative.");

        RuleFor(command => command.BaseNightlyRateCurrency)
            .NotEmpty()
            .WithMessage("Base nightly rate currency must be provided.")
            .Must(IsSupportedCurrency)
            .WithMessage("Base nightly rate currency is not supported.");
    }

    private static bool IsSupportedCurrency(string currency)
    {
        return Enum.TryParse(currency, ignoreCase: true, out Currency parsedCurrency) &&
            Enum.IsDefined(parsedCurrency);
    }
}
