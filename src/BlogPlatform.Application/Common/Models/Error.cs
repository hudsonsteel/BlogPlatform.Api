namespace BlogPlatform.Application.Common.Models;

public enum ErrorType
{
    None = 0,
    Validation,
    NotFound,
    Conflict,
    Unexpected,
}

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Validation(string message, string code = "Validation") =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string message, string code = "NotFound") =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string message, string code = "Conflict") =>
        new(code, message, ErrorType.Conflict);

    public static Error Unexpected(string message, string code = "Unexpected") =>
        new(code, message, ErrorType.Unexpected);
}
