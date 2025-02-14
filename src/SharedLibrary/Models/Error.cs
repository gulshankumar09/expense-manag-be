using SharedLibrary.Constants;

namespace SharedLibrary.Models;

/// <summary>
/// Represents an error with code and message
/// </summary>
public class Error
{
    /// <summary>
    /// Gets the HTTP status code
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the error code string
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets additional error details (optional)
    /// </summary>
    public object? Details { get; }

    private Error(int statusCode, string code, string message, object? details = null)
    {
        StatusCode = statusCode;
        Code = code;
        Message = message;
        Details = details;
    }

    /// <summary>
    /// Creates a new Error instance
    /// </summary>
    public static Error Create(int statusCode, string code, string message, object? details = null)
        => new(statusCode, code, message, details);

    /// <summary>
    /// Creates a validation error
    /// </summary>
    public static Error Validation(string message = ErrorConstants.Messages.ValidationError, object? details = null)
        => new(400, ErrorConstants.Codes.ValidationCode, message, details);

    /// <summary>
    /// Creates a not found error
    /// </summary>
    public static Error NotFound(string message = ErrorConstants.Messages.ResourceNotFound)
        => new(404, ErrorConstants.Codes.NotFoundCode, message);

    /// <summary>
    /// Creates an unauthorized error
    /// </summary>
    public static Error Unauthorized(string message = ErrorConstants.Messages.UnauthorizedAccess)
        => new(401, ErrorConstants.Codes.UnauthorizedCode, message);

    /// <summary>
    /// Creates a forbidden error
    /// </summary>
    public static Error Forbidden(string message = ErrorConstants.Messages.UnauthorizedAccess)
        => new(403, ErrorConstants.Codes.ForbiddenCode, message);

    /// <summary>
    /// Creates a conflict error
    /// </summary>
    public static Error Conflict(string message, object? details = null)
        => new(409, ErrorConstants.Codes.ConflictCode, message, details);

    /// <summary>
    /// Creates a bad request error
    /// </summary>
    public static Error BadRequest(string message = ErrorConstants.Messages.InvalidRequest, object? details = null)
        => new(400, ErrorConstants.Codes.BadRequestCode, message, details);

    /// <summary>
    /// Creates an internal server error
    /// </summary>
    public static Error InternalServerError(string message = ErrorConstants.Messages.InternalServerError, object? details = null)
        => new(500, ErrorConstants.Codes.InternalErrorCode, message, details);

    public override string ToString() => $"{StatusCode} {Code}: {Message}";
}