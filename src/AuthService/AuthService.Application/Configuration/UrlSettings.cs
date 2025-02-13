namespace AuthService.Application.Configuration;

/// <summary>
/// Configuration settings for application URLs
/// </summary>
public class UrlSettings
{
    public const string SectionName = "Urls";

    /// <summary>
    /// Base URL for the web application
    /// </summary>
    public string WebAppBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL patterns for various application endpoints
    /// </summary>
    public EndpointUrls Endpoints { get; set; } = new();

    /// <summary>
    /// URL patterns for email verification and password reset
    /// </summary>
    public EmailUrls Email { get; set; } = new();
}

public class EndpointUrls
{
    /// <summary>
    /// URL pattern for email verification
    /// </summary>
    public string VerifyEmail { get; set; } = "/verify-email";

    /// <summary>
    /// URL pattern for password reset
    /// </summary>
    public string ResetPassword { get; set; } = "/reset-password";

    /// <summary>
    /// URL pattern for OTP verification
    /// </summary>
    public string VerifyOtp { get; set; } = "/verify-otp";
}

public class EmailUrls
{
    /// <summary>
    /// Complete URL for email verification
    /// </summary>
    public string VerificationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Complete URL for password reset
    /// </summary>
    public string PasswordResetUrl { get; set; } = string.Empty;

    /// <summary>
    /// Complete URL for OTP verification
    /// </summary>
    public string OtpVerificationUrl { get; set; } = string.Empty;
}