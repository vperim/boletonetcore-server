using BoletoNetCore.Server.Configuration;

namespace BoletoNetCore.Server.Hosting;

/// <summary>
/// Prints application configuration on startup for diagnostics.
/// </summary>
public sealed class ConfigPrinterHostedService : IHostedService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<ConfigPrinterHostedService> logger;

    // Substrings to detect sensitive configuration keys (case-insensitive).
    // Uses substring matching to catch nested keys like "ApiKey:Key".
    private static readonly string[] SensitiveKeyPatterns =
    [
        "Key",
        "Password",
        "Secret",
        "Token",
        "ConnectionString"
    ];

    public ConfigPrinterHostedService(IConfiguration configuration, ILogger<ConfigPrinterHostedService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("=== Application Configuration ===");

        PrintSimpleValue("AllowedHosts");
        PrintSection(ApiKeyOptions.SectionName);
        PrintSection(ReverseProxyOptions.SectionName);
        PrintSection(GrpcWebSettings.SectionName);
        PrintSection("PlaywrightRenderer");
        PrintKestrelEndpoints();
        PrintSerilogMinimumLevel();

        this.logger.LogInformation("=== End Configuration ===");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void PrintSimpleValue(string key)
    {
        var value = this.configuration[key];
        if (!string.IsNullOrEmpty(value))
            this.logger.LogInformation("{Key} = {Value}", key, value);
    }

    private void PrintSection(string sectionName)
    {
        var section = this.configuration.GetSection(sectionName);
        if (!section.Exists())
            return;

        foreach (var child in section.GetChildren())
            PrintConfigValue(sectionName, child);
    }

    private void PrintConfigValue(string prefix, IConfigurationSection section)
    {
        var fullKey = $"{prefix}:{section.Key}";

        if (section.Value is not null)
        {
            var displayValue = IsSensitive(section.Key) ? "***" : section.Value;
            this.logger.LogInformation("{Key} = {Value}", fullKey, displayValue);
        }

        foreach (var child in section.GetChildren())
            PrintConfigValue(fullKey, child);
    }

    private void PrintKestrelEndpoints()
    {
        var endpoints = this.configuration.GetSection("Kestrel:Endpoints");
        if (!endpoints.Exists())
            return;

        foreach (var endpoint in endpoints.GetChildren())
        {
            var url = endpoint["Url"];
            var protocols = endpoint["Protocols"];
            if (!string.IsNullOrEmpty(url))
                this.logger.LogInformation("Kestrel:Endpoints:{Endpoint} = {Url} ({Protocols})",
                    endpoint.Key, url, protocols ?? "default");
        }
    }

    private void PrintSerilogMinimumLevel()
    {
        var defaultLevel = this.configuration["Serilog:MinimumLevel:Default"];
        if (!string.IsNullOrEmpty(defaultLevel))
            this.logger.LogInformation("Serilog:MinimumLevel:Default = {Value}", defaultLevel);
    }

    private static bool IsSensitive(string key) =>
        SensitiveKeyPatterns.Any(pattern => key.Contains(pattern, StringComparison.OrdinalIgnoreCase));
}
