namespace SharedLibrary.Configuration;

/// <summary>
/// Configuration settings for CORS (Cross-Origin Resource Sharing)
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// The name of the CORS policy
    /// </summary>
    public string PolicyName { get; set; } = "DefaultPolicy";

    /// <summary>
    /// List of allowed origins
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of allowed HTTP methods
    /// </summary>
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of allowed headers
    /// </summary>
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether to allow credentials (cookies, authorization headers)
    /// </summary>
    public bool AllowCredentials { get; set; }

    /// <summary>
    /// Maximum age of preflight requests in seconds
    /// </summary>
    public int PreflightMaxAge { get; set; } = 600; // 10 minutes
} 