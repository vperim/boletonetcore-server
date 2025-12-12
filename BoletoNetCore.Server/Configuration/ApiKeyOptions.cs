namespace BoletoNetCore.Server.Configuration;

/// <summary>
/// Configuration options for API key authentication.
/// </summary>
public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";
    public const string HeaderName = "X-API-Key";

    /// <summary>
    /// Enable API key authentication.
    /// Default: false
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The API key value. Required when Enabled is true.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}
