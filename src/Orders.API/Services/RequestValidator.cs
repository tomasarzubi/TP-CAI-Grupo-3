using System.ComponentModel.DataAnnotations;

namespace Orders.API.Services;

public static class RequestValidator
{
    public static void Validate(object request, string errorCode)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);

        if (Validator.TryValidateObject(request, context, results, validateAllProperties: true))
            return;

        throw new Exceptions.ValidationException(
            errorCode, string.Join("; ", results.Select(r => r.ErrorMessage)));
    }
}
