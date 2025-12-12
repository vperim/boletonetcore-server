namespace BoletoNetCore.Server.Configuration;

/// <summary>
/// Configuration options for gRPC-Web support.
/// </summary>
public sealed class GrpcWebSettings
{
    public const string SectionName = "GrpcWeb";

    /// <summary>
    /// Enable gRPC-Web support for browser clients.
    /// Standard gRPC clients are unaffected when enabled.
    /// Default: false
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Enable CORS for gRPC-Web requests.
    /// Required when browser clients are on a different origin.
    /// Default: false
    /// </summary>
    public bool EnableCors { get; set; }

    /// <summary>
    /// Allowed origins for CORS. Empty allows all origins.
    /// Only used when EnableCors is true.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];
}
