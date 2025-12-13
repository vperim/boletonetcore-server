using System.Net;
using BoletoNetCore.Server.Configuration;
using BoletoNetCore.Server.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using HttpOverridesIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace BoletoNetCore.Server.Extensions;

/// <summary>
/// Extension methods for infrastructure configuration (reverse proxy, gRPC-Web, health checks).
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Configures reverse proxy support based on appsettings.
    /// Options are read at runtime via IConfiguration to support test configuration overrides.
    /// </summary>
    public static IServiceCollection ConfigureReverseProxy(this IServiceCollection services)
    {
        // Use PostConfigure to read configuration at runtime (after all configuration sources are loaded)
        services.AddOptions<ForwardedHeadersOptions>()
            .Configure<IConfiguration>((fwdOptions, configuration) =>
            {
                var options = configuration
                    .GetSection(ReverseProxyOptions.SectionName)
                    .Get<ReverseProxyOptions>() ?? new ReverseProxyOptions();

                // Only configure if enabled
                if (!options.Enabled)
                {
                    return;
                }

                fwdOptions.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                // Cloudflare uses CF-Connecting-IP header
                if (options.UseCloudflare)
                {
                    fwdOptions.ForwardedForHeaderName = "CF-Connecting-IP";
                }

                fwdOptions.ForwardLimit = options.ForwardLimit;

                // Clear defaults and add configured proxies
                if (options.KnownProxies.Length > 0 || options.KnownNetworks.Length > 0)
                {
                    fwdOptions.KnownProxies.Clear();
                    fwdOptions.KnownNetworks.Clear();

                    foreach (var proxy in options.KnownProxies)
                    {
                        if (IPAddress.TryParse(proxy, out var ip))
                        {
                            fwdOptions.KnownProxies.Add(ip);
                        }
                    }

                    foreach (var network in options.KnownNetworks)
                    {
                        var parts = network.Split('/');
                        if (parts.Length == 2 &&
                            IPAddress.TryParse(parts[0], out var prefix) &&
                            int.TryParse(parts[1], out var prefixLength))
                        {
                            fwdOptions.KnownNetworks.Add(new HttpOverridesIPNetwork(prefix, prefixLength));
                        }
                    }
                }

                // Allowed hosts for security
                foreach (var host in options.AllowedHosts)
                {
                    fwdOptions.AllowedHosts.Add(host);
                }
            });

        return services;
    }

    /// <summary>
    /// Configures gRPC-Web CORS support based on appsettings.
    /// </summary>
    public static IServiceCollection ConfigureGrpcWeb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(GrpcWebSettings.SectionName)
            .Get<GrpcWebSettings>() ?? new GrpcWebSettings();

        if (!settings.Enabled || !settings.EnableCors)
        {
            return services;
        }

        services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy("GrpcWeb", builder =>
            {
                var origins = settings.AllowedOrigins.Length > 0
                    ? builder.WithOrigins(settings.AllowedOrigins)
                    : builder.AllowAnyOrigin();

                origins
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(
                        "Grpc-Status",
                        "Grpc-Message",
                        "Grpc-Encoding",
                        "Grpc-Accept-Encoding",
                        "Grpc-Status-Details-Bin");
            });
        });

        return services;
    }

    /// <summary>
    /// Configures gRPC health checks for Docker/Kubernetes.
    /// </summary>
    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services)
    {
        services.AddGrpcHealthChecks()
            .AddCheck("liveness", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

        return services;
    }

    /// <summary>
    /// Uses reverse proxy middleware if enabled in configuration.
    /// </summary>
    public static IApplicationBuilder UseReverseProxyIfEnabled(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(ReverseProxyOptions.SectionName)
            .Get<ReverseProxyOptions>() ?? new ReverseProxyOptions();

        if (options.Enabled)
        {
            app.UseForwardedHeaders();
        }

        return app;
    }

    /// <summary>
    /// Uses gRPC-Web middleware if enabled in configuration.
    /// </summary>
    public static IApplicationBuilder UseGrpcWebIfEnabled(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(GrpcWebSettings.SectionName)
            .Get<GrpcWebSettings>() ?? new GrpcWebSettings();

        if (settings.Enabled)
        {
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

            if (settings.EnableCors)
            {
                app.UseCors("GrpcWeb");
            }
        }

        return app;
    }

    /// <summary>
    /// Uses API key authentication middleware if enabled in configuration.
    /// </summary>
    public static IApplicationBuilder UseApiKeyAuthIfEnabled(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(ApiKeyOptions.SectionName)
            .Get<ApiKeyOptions>() ?? new ApiKeyOptions();

        if (options.Enabled)
        {
            app.UseMiddleware<ApiKeyMiddleware>(options);
        }

        return app;
    }
}
