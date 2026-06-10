using FluentValidation;

namespace HotelPms.Shared.Api;

public static class ValidationProblemMapper
{
    public static Dictionary<string, string[]> ToErrors(
        ValidationException exception,
        string fallbackPropertyName)
    {
        if (exception.Errors.Any())
        {
            return exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());
        }

        return new Dictionary<string, string[]>
        {
            [fallbackPropertyName] = [exception.Message]
        };
    }
}
