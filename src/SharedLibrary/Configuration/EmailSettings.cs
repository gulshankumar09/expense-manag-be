namespace SharedLibrary.Configuration;

/// <summary>
/// Configuration settings for email service
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// The section name in the configuration file
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// SMTP server host
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Whether to use SSL/TLS
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// SMTP authentication username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// SMTP authentication password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Sender email address
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Sender display name
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum size for attachments in bytes
    /// </summary>
    public long MaxAttachmentSize { get; set; } = 10 * 1024 * 1024; // 10MB default

    /// <summary>
    /// Timeout for SMTP operations in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum recipients per email
    /// </summary>
    public int MaxRecipientsPerEmail { get; set; } = 50;
}