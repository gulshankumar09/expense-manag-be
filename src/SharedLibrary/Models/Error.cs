using SharedLibrary.Constants;

namespace SharedLibrary.Models;

/// <summary>
/// Represents an error with code and message
/// </summary>
public class Error
{
    /// <summary>
    /// Gets the string error code
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Gets the error message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets additional error details (optional)
    /// </summary>
    public object? Details { get; }

    private Error(int code, string message, object? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }

    /// <summary>
    /// Creates a new Error instance
    /// </summary>
    public static Error Create(int code, string message, object? details = null)
        => new(code, message, details);

    /// <summary>
    /// Creates a validation error
    /// </summary>
    public static Error Validation(string message, object? details = null)
        => new(ErrorConstants.Codes.ValidationErrorCode, ErrorConstants.Codes.ValidationError, details);

    /// <summary>
    /// Creates a not found error
    /// </summary>
    public static Error NotFound()
        => new(ErrorConstants.Codes.NotFoundCode, ErrorConstants.Codes.NotFound);

    /// <summary>
    /// Creates an unauthorized error
    /// </summary>
    public static Error Unauthorized()
        => new(ErrorConstants.Codes.UnauthorizedCode, ErrorConstants.Codes.Unauthorized);

    /// <summary>
    /// Creates a conflict error
    /// </summary>
    public static Error Conflict()
        => new(ErrorConstants.Codes.ConflictCode, ErrorConstants.Codes.Conflict);

    /// <summary>
    /// Creates a bad request error
    /// </summary>
    public static Error BadRequest(object? details = null)
        => new(ErrorConstants.Codes.BadRequestCode, ErrorConstants.Codes.BadRequest, details);

    /// <summary>
    /// Creates an internal server error
    /// </summary>
    public static Error InternalServerError(object? details = null)
        => new(ErrorConstants.Codes.InternalErrorCode, ErrorConstants.Codes.InternalError, details);

    public override string ToString() => $"{Code}: {Message}";
}