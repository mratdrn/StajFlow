namespace StajFlow.Api.Services;

public record RegistrationResult(bool Succeeded, IDictionary<string, string[]> Errors)
{
    public static RegistrationResult Success() => new(true, new Dictionary<string, string[]>());
    public static RegistrationResult Invalid(string field, string message) =>
        new(false, new Dictionary<string, string[]> { [field] = [message] });
}

