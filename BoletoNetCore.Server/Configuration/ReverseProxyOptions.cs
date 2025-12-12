namespace BoletoNetCore.Server.Configuration;

/// <summary>
/// Configuration options for reverse proxy support.
/// </summary>
public sealed class ReverseProxyOptions
{
    public const string SectionName = "ReverseProxy";

    /// <summary>
    /// Enable reverse proxy header processing.
    /// Default: false
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// List of known proxy IP addresses.
    /// Only headers from these IPs will be processed.
    /// </summary>
    public string[] KnownProxies { get; set; } = [];

    /// <summary>
    /// List of known proxy networks in CIDR notation (e.g., "10.0.0.0/8").
    /// </summary>
    public string[] KnownNetworks { get; set; } = [];

    /// <summary>
    /// Allowed host names. Empty allows all hosts.
    /// </summary>
    public string[] AllowedHosts { get; set; } = [];

    /// <summary>
    /// Enable Cloudflare-specific header handling (CF-Connecting-IP).
    /// </summary>
    public bool UseCloudflare { get; set; }

    /// <summary>
    /// Maximum number of forwarded header entries to process.
    /// Default: 1
    /// </summary>
    public int ForwardLimit { get; set; } = 1;
}
