using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedLibrary.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using SharedLibrary.Exceptions;

namespace SharedLibrary.Middleware;

/// <summary>
/// Middleware for handling exceptions globally across the application
/// </summary>
public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception error)
        {
            await HandleException(context, error);
        }
    }

    private async Task HandleException(HttpContext context, Exception error)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var stackTrace = env.IsDevelopment() ? new { error.StackTrace } : null;

        var result = error switch
        {
            BaseException baseEx => Result.Failure(
                Error.Create(baseEx.StatusCode, baseEx.Code, baseEx.Message, baseEx.Details)),

            UnauthorizedAccessException => Result.Failure(
                Error.Unauthorized(error.Message)),

            KeyNotFoundException => Result.Failure(
                Error.NotFound(error.Message)),

            ArgumentException when error.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => Result.Failure(Error.NotFound(error.Message)),

            ArgumentException => Result.Failure(
                Error.BadRequest(error.Message)),

            InvalidOperationException => Result.Failure(
                Error.BadRequest(error.Message, stackTrace)),

            _ => Result.Failure(
                Error.InternalServerError(error.Message, stackTrace))
        };

        var err = (Error)result.Error!;

        // Add standard extensions for all errors
        err.Extensions.Add("type", error.GetType().Name);
        err.Extensions.Add("traceId", Activity.Current?.Id);
        err.Extensions.Add("requestId", context.TraceIdentifier);
        err.Extensions.Add("instance", $"{context.Request.Method} {context.Request.Path}");
        err.Extensions.Add("query", context.Request.QueryString.ToString());
        err.Extensions.Add("host", context.Request.Host.ToString());
        err.Extensions.Add("protocol", context.Request.Protocol);

        // Set the response status code from the error
        response.StatusCode = err.StatusCode;

        // Log the error with appropriate level based on status code
        if (response.StatusCode >= 500)
        {
            logger.LogError(error, "An unhandled exception occurred: {Message}", error.Message);
        }
        else
        {
            logger.LogWarning("An error occurred: {Message}", error.Message);
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
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