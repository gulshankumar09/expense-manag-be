using System.Collections.Generic;

namespace SharedLibrary.Exceptions;

/// <summary>
/// Base exception class for custom application exceptions
/// </summary>
public class BaseException : Exception
{
    private const string StatusCodeKey = "StatusCode";
    private const string ErrorCodeKey = "Code";
    private const string DetailsKey = "Details";

    /// <summary>
    /// Gets the HTTP status code
    /// </summary>
    public int StatusCode => (int)(Data[StatusCodeKey] ?? 500);

    /// <summary>
    /// Gets the error code string
    /// </summary>
    public string Code => (string)(Data[ErrorCodeKey] ?? "INTERNAL_ERROR");

    /// <summary>
    /// Gets additional error details (optional)
    /// </summary>
    public object? Details => Data[DetailsKey];

    /// <summary>
    /// Initializes a new instance of BaseException
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    /// <param name="details">Additional details (optional)</param>
    public BaseException(int statusCode, string code, string message, object? details = null)
        : base(message)
    {
        Data[StatusCodeKey] = statusCode;
        Data[ErrorCodeKey] = code;
        if (details != null) Data[DetailsKey] = details;
    }
}