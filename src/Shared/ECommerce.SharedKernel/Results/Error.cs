namespace ECommerce.SharedKernel.Results;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error New(string code, string message) => new(code, message);
    public static Error NotFound(string resource) => new($"{resource}.NotFound", $"{resource} was not found.");
    public static Error Conflict(string resource) => new($"{resource}.Conflict", $"{resource} already exists.");
    public static Error Validation(string field, string message) => new($"Validation.{field}", message);
}
