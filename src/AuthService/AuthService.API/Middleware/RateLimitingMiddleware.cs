using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AuthService.API.Models;

namespace AuthService.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint()?.DisplayName;
            if (endpoint != null && endpoint.Contains("verify-otp"))
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var bucket = _buckets.GetOrAdd(ip, _ => new TokenBucket(5, TimeSpan.FromMinutes(15)));

                if (!bucket.TryTake())
                {
                    _logger.LogWarning("Rate limit exceeded for IP: {IP}", ip);
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsJsonAsync(new { error = "Too many requests" });
                    return;
                }
            }

            await _next(context);
        }
    }
} 