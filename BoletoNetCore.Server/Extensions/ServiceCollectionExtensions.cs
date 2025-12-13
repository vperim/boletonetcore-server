using BoletoNetCore.Server.Interceptors;
using BoletoNetCore.Server.Services.Boletos;
using BoletoNetCore.Server.Services.Boletos.Rendering;
using BoletoNetCore.Server.Versioning;
using Microsoft.OpenApi.Models;
using Serilog;

namespace BoletoNetCore.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCoreServices(this IServiceCollection services)
    {
        return services.AddSingleton(TimeProvider.System);
    }

    public static IServiceCollection ConfigureBoletoServices(this IServiceCollection services)
    {
        services.AddScoped<IBancoFactory, BancoFactory>();
        services.AddScoped<IBoletoGenerator, BoletoGenerator>();
        services.AddScoped<IBoletoOutputRenderer, PdfBoletoRenderer>();
        services.AddScoped<IOutputRendererFactory, OutputRendererFactory>();
        return services;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddLogging(options =>
        {
            options.ClearProviders();
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            options.AddSerilog(logger);
        });
    }

    public static IServiceCollection ConfigureGrpc(this IServiceCollection services)
    {
        // Enable gRPC with detailed errors & Swagger for REST clients
        services.AddGrpc(o =>
        {
            o.EnableDetailedErrors = true;
            o.Interceptors.Add<LoggerInterceptor>();
        }).AddJsonTranscoding();
        services.AddGrpcSwagger();
        return services;
    }

    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        // Register API Versions in Swagger
        return services.AddSwaggerGen(c =>
        {
            foreach (var version in ApiVersions.Versions)
                c.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = $"BoletoNetCore - Server ({version.ToUpper()})",
                    Version = version
                });

            c.OperationFilter<SwaggerVersioningOperationFilter>();
            c.OperationFilter<SwaggerDefaultRequestFilter>();
        });
    }
}