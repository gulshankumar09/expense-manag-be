using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedLibrary.Models;
using SharedLibrary.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace SharedLibrary.Middleware;

/// <summary>
/// Middleware for handling exceptions globally across the application
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var result = error switch
            {
                UnauthorizedAccessException => Result.Failure(
                    Error.Unauthorized(error.Message)),

                KeyNotFoundException => Result.Failure(
                    Error.NotFound(error.Message)),

                ArgumentException when error.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    => Result.Failure(Error.NotFound(error.Message)),

                ArgumentException => Result.Failure(
                    Error.BadRequest(error.Message)),

                InvalidOperationException => Result.Failure(
                    Error.BadRequest(error.Message)),

                _ => Result.Failure(
                    Error.InternalServerError(
                        error.Message,
                        _env.IsDevelopment() ? new { error.StackTrace } : null))
            };

            // Set the response status code from the error
            response.StatusCode = ((Error)result.Error!).StatusCode;

            // Log the error with appropriate level based on status code
            if (response.StatusCode >= 500)
            {
                _logger.LogError(error, "An unhandled exception occurred: {Message}", error.Message);
            }
            else
            {
                _logger.LogWarning("An error occurred: {Message}", error.Message);
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
        }
    }
}

/// <summary>
/// Extension methods for configuring the ExceptionMiddleware
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    /// <summary>
    /// Adds the global exception handling middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}