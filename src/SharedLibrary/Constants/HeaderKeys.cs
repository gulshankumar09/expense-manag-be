namespace SharedLibrary.Constants;

/// <summary>
/// Contains constant values for custom HTTP header keys used across the application
/// </summary>
public static class HeaderKeys
{
    /// <summary>
    /// Header key for redirect URL
    /// </summary>
    public const string RedirectUrl = "X-Redirect-Url";

    /// <summary>
    /// Header key for authentication token
    /// </summary>
    public const string AuthToken = "X-Auth-Token";

    /// <summary>
    /// Header key for refresh token
    /// </summary>
    public const string RefreshToken = "X-Refresh-Token";

    /// <summary>
    /// Header key for correlation ID
    /// </summary>
    public const string CorrelationId = "X-Correlation-Id";

    /// <summary>
    /// Header key for request ID
    /// </summary>
    public const string RequestId = "X-Request-Id";
}