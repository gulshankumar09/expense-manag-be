using SharedLibrary.Constants;

namespace SharedLibrary.Exceptions;

/// <summary>
/// Exception for handling email-related errors across different services
/// </summary>
public class EmailException : BaseException
{
    /// <summary>
    /// Creates a new instance of EmailException for invalid email configuration
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="details">Additional error details (optional)</param>
    public static EmailException InvalidConfiguration(string message, object? details = null)
        => new(500, ErrorConstants.Codes.EmailConfigError, message, details);

    /// <summary>
    /// Creates a new instance of EmailException for SMTP connection errors
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="details">Additional error details (optional)</param>
    public static EmailException SmtpConnectionError(string message, object? details = null)
        => new(500, ErrorConstants.Codes.SmtpConnectionError, message, details);

    /// <summary>
    /// Creates a new instance of EmailException for authentication errors
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="details">Additional error details (optional)</param>
    public static EmailException AuthenticationError(string message, object? details = null)
        => new(500, ErrorConstants.Codes.EmailAuthError, message, details);

    /// <summary>
    /// Creates a new instance of EmailException for invalid recipient errors
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="details">Additional error details (optional)</param>
    public static EmailException InvalidRecipient(string message, object? details = null)
        => new(400, ErrorConstants.Codes.InvalidEmailRecipient, message, details);

    /// <summary>
    /// Creates a new instance of EmailException for rate limiting errors
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="details">Additional error details (optional)</param>
    public static EmailException RateLimitExceeded(string message, object? details = null)
        => new(429, ErrorConstants.Codes.EmailRateLimit, message, details);

    /// <summary>
    /// Creates a new instance of EmailException for general sending errors
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="details">Additional error details (optional)</param>
    public static EmailException SendingError(string message, object? details = null)
        => new(500, ErrorConstants.Codes.EmailSendingError, message, details);

    /// <summary>
    /// Creates a new instance of EmailException for template rendering errors
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="details">Additional error details (optional)</param>
    public static EmailException TemplateError(string message, object? details = null)
        => new(500, ErrorConstants.Codes.EmailTemplateError, message, details);

    /// <summary>
    /// Creates a new instance of EmailException for attachment errors
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="details">Additional error details (optional)</param>
    public static EmailException AttachmentError(string message, object? details = null)
        => new(400, ErrorConstants.Codes.EmailAttachmentError, message, details);

    /// <summary>
    /// Private constructor to enforce the use of static factory methods
    /// </summary>
    private EmailException(int statusCode, string code, string message, object? details = null)
        : base(statusCode, code, message, details)
    {
    }
}