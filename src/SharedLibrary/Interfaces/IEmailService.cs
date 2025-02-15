namespace SharedLibrary.Interfaces;

/// <summary>
/// Interface for email service operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to the specified recipient
    /// </summary>
    /// <param name="to">The recipient's email address</param>
    /// <param name="subject">The email subject</param>
    /// <param name="body">The email body content (can be HTML)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Sends an email to multiple recipients
    /// </summary>
    /// <param name="to">List of recipient email addresses</param>
    /// <param name="subject">The email subject</param>
    /// <param name="body">The email body content (can be HTML)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendEmailAsync(IEnumerable<string> to, string subject, string body);

    /// <summary>
    /// Sends an email with attachments
    /// </summary>
    /// <param name="to">The recipient's email address</param>
    /// <param name="subject">The email subject</param>
    /// <param name="body">The email body content (can be HTML)</param>
    /// <param name="attachments">Dictionary of attachment file names and their byte content</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendEmailWithAttachmentsAsync(string to, string subject, string body, IDictionary<string, byte[]> attachments);
}