using BoletoNetCore.Server.Extensions;
using BoletoNetCore.Server.Services.V1;
using BoletoNetCore.Server.Versioning;

namespace BoletoNetCore.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables(prefix: "BOLETO_");
        Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);

        // Kestrel is configured via appsettings.json:
        // - HTTP/1.1 for browsers (Swagger)
        // - HTTP/2 for gRPC clients

        builder.Host.UseDefaultServiceProvider((_, options) =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        builder.Services
            .ConfigureCoreServices()
            .ConfigureBoletoServices(builder.Configuration)
            .ConfigureLogging(builder.Configuration)
            .ConfigureGrpc()
            .ConfigureSwagger()
            .ConfigureReverseProxy()
            .ConfigureGrpcWeb(builder.Configuration)
            .ConfigureHealthChecks();

        var app = builder.Build();

        // Reverse proxy headers (must be before other middleware)
        app.UseReverseProxyIfEnabled(builder.Configuration);

        // API key authentication (after reverse proxy, before other middleware)
        app.UseApiKeyAuthIfEnabled(builder.Configuration);

        // Swagger UI and diagnostic endpoints (development only)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var version in ApiVersions.Versions)
                    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"BoletoNetCore Server {version.ToUpper()}");
            });

            app.MapGet("/_diagnostics/connection", (HttpContext ctx) => new
            {
                RemoteIp = ctx.Connection.RemoteIpAddress?.ToString(),
                ctx.Request.Scheme,
                Host = ctx.Request.Host.ToString()
            });
        }

        // gRPC-Web support (if enabled)
        app.UseGrpcWebIfEnabled(builder.Configuration);

        app.MapGet("/", () => "BoletoNetCore.Server is running");
        app.MapHealthChecks("/health");

        // Map gRPC services
        app.MapGrpcService<BoletoV1Service>();
        app.MapGrpcHealthChecksService();

        app.Run();
    }
}
