using BoletoNetCore.Server.Configuration;
using Grpc.Core;

namespace BoletoNetCore.Server.Middleware;

/// <summary>
/// Middleware for API key authentication.
/// Validates X-API-Key header for all requests except health checks.
/// </summary>
public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate next;
    private readonly ApiKeyOptions options;
    private readonly ILogger<ApiKeyMiddleware> logger;

    private static readonly HashSet<string> BypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/health",
        "/grpc.health.v1.Health/Check",
        "/grpc.health.v1.Health/Watch"
    };

    public ApiKeyMiddleware(
        RequestDelegate next,
        ApiKeyOptions options,
        ILogger<ApiKeyMiddleware> logger)
    {
        this.next = next;
        this.options = options;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!this.options.Enabled)
        {
            await this.next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        if (ShouldBypass(path))
        {
            await this.next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyOptions.HeaderName, out var providedKey))
        {
            this.logger.LogWarning("API key missing for request to {Path}", path);
            await WriteUnauthorizedResponse(context, "API key is required");
            return;
        }

        if (!string.Equals(providedKey, this.options.Key, StringComparison.Ordinal))
        {
            this.logger.LogWarning("Invalid API key provided for request to {Path}", path);
            await WriteUnauthorizedResponse(context, "Invalid API key");
            return;
        }

        await this.next(context);
    }

    private static bool ShouldBypass(string path)
    {
        return BypassPaths.Contains(path) || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        var contentType = context.Request.ContentType ?? string.Empty;

        if (contentType.StartsWith("application/grpc", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.ContentType = "application/grpc";
            context.Response.Headers.Append("grpc-status", ((int)StatusCode.Unauthenticated).ToString());
            context.Response.Headers.Append("grpc-message", message);
            context.Response.StatusCode = StatusCodes.Status200OK;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\":\"{message}\"}}");
        }
    }
}
