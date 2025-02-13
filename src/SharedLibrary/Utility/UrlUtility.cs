using Microsoft.AspNetCore.Http;

namespace SharedLibrary.Utility;

/// <summary>
/// Utility class for URL-related operations
/// </summary>
public static class UrlUtility
{
    /// <summary>
    /// Gets the current absolute URL from the HttpContext
    /// </summary>
    /// <param name="request">The HttpRequest object</param>
    /// <returns>The current absolute URL</returns>
    public static string GetCurrentUrl(HttpRequest request)
    {
        return $"{GetBaseUrl(request)}{request.Path}{request.QueryString}";
    }

    /// <summary>
    /// Gets the base URL (scheme + host) from the HttpContext
    /// </summary>
    /// <param name="request">The HttpRequest object</param>
    /// <returns>The base URL</returns>
    public static string GetBaseUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}";
    }

    /// <summary>
    /// Combines multiple URL segments into a single URL, handling slashes appropriately
    /// </summary>
    /// <param name="baseUrl">The base URL</param>
    /// <param name="segments">Additional URL segments to combine</param>
    /// <returns>The combined URL</returns>
    public static string CombineUrl(string baseUrl, params string[] segments)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));

        var url = baseUrl.TrimEnd('/');

        foreach (var segment in segments)
        {
            if (string.IsNullOrEmpty(segment))
                continue;

            url = $"{url}/{segment.Trim('/')}";
        }

        return url;
    }

    /// <summary>
    /// Adds query parameters to a URL
    /// </summary>
    /// <param name="url">The base URL</param>
    /// <param name="queryParams">Dictionary of query parameters</param>
    /// <returns>URL with query parameters</returns>
    public static string AddQueryParams(string url, IDictionary<string, string> queryParams)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentNullException(nameof(url));

        if (queryParams == null || !queryParams.Any())
            return url;

        var uriBuilder = new UriBuilder(url);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var param in queryParams)
        {
            query[param.Key] = param.Value;
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    /// <summary>
    /// Ensures a URL starts with a scheme (http:// or https://)
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <param name="defaultScheme">The default scheme to use if none is present (defaults to https)</param>
    /// <returns>URL with scheme</returns>
    public static string EnsureScheme(string url, string defaultScheme = "https")
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentNullException(nameof(url));

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        return $"{defaultScheme}://{url.TrimStart('/')}";
    }

    /// <summary>
    /// Gets the absolute URL for a given relative path
    /// </summary>
    /// <param name="request">The HttpRequest object</param>
    /// <param name="relativePath">The relative path</param>
    /// <returns>The absolute URL</returns>
    public static string GetAbsoluteUrl(HttpRequest request, string relativePath)
    {
        return CombineUrl(GetBaseUrl(request), relativePath);
    }

    /// <summary>
    /// Checks if a URL is absolute (contains scheme and host)
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <returns>True if the URL is absolute, false otherwise</returns>
    public static bool IsAbsoluteUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}