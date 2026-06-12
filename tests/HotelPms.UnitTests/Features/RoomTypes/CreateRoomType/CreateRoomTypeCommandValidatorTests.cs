using FluentValidation.Results;
using HotelPms.Features.RoomTypes.CreateRoomType;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.UnitTests.Features.RoomTypes.CreateRoomType;

public class CreateRoomTypeCommandValidatorTests
{
    private readonly CreateRoomTypeCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ReturnsNoErrors()
    {
        CreateRoomTypeCommand command = CreateValidCommand();

        ValidationResult result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NegativeBaseNightlyRateAmount_ReturnsBaseNightlyRateAmountError()
    {
        CreateRoomTypeCommand command = CreateValidCommand(baseNightlyRateAmount: -1);

        ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "BaseNightlyRateAmount");
    }

    [Fact]
    public void Validate_UnsupportedBaseNightlyRateCurrency_ReturnsBaseNightlyRateCurrencyError()
    {
        CreateRoomTypeCommand command = CreateValidCommand(baseNightlyRateCurrency: "EUR");

        ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "BaseNightlyRateCurrency");
    }

    [Fact]
    public void Validate_UndefinedNumericBaseNightlyRateCurrency_ReturnsBaseNightlyRateCurrencyError()
    {
        CreateRoomTypeCommand command = CreateValidCommand(baseNightlyRateCurrency: "999");

        ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "BaseNightlyRateCurrency");
    }

    private static CreateRoomTypeCommand CreateValidCommand(
        decimal baseNightlyRateAmount = 120_000,
        string baseNightlyRateCurrency = "KRW")
    {
        return new CreateRoomTypeCommand(
            TenantId.New(),
            "DBL",
            "Double",
            2,
            4,
            baseNightlyRateAmount,
            baseNightlyRateCurrency);
    }
}
