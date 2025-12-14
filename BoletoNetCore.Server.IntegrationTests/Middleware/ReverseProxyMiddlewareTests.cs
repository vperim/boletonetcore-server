using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BoletoNetCore.Server.IntegrationTests.Middleware;

/// <summary>
/// Integration tests for reverse proxy middleware configuration.
/// Uses the /_diagnostics/connection endpoint to verify forwarded headers are processed.
///
/// Note: WebApplicationFactory creates an in-memory server with no TCP connection,
/// so RemoteIpAddress is null. Tests that verify actual IP forwarding use no KnownProxies
/// configuration, allowing the middleware to process all requests (permissive mode).
/// Tests for KnownProxies/KnownNetworks configuration verify the server starts correctly
/// with various configurations but cannot verify actual proxy filtering behavior.
/// </summary>
[Trait("Category", "Middleware")]
public sealed class ReverseProxyMiddlewareTests : IAsyncDisposable
{
    private const string DiagnosticEndpoint = "/_diagnostics/connection";
    private const string TestClientIp = "203.0.113.50";
    private const string TestProxyIp = "10.0.0.1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private WebApplicationFactory<Program>? factory;
    private HttpClient? httpClient;

    public async ValueTask DisposeAsync()
    {
        this.httpClient?.Dispose();

        if (this.factory is not null)
        {
            await this.factory.DisposeAsync();
        }
    }

    private void CreateFactory(Dictionary<string, string?> config)
    {
        this.factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        this.httpClient = this.factory.CreateClient();
    }

    private async Task<ConnectionInfo> GetConnectionInfoAsync(HttpRequestMessage request)
    {
        var response = await this.httpClient!.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConnectionInfo>(json, JsonOptions)!;
    }

    #region Disabled Mode

    [Fact]
    public async Task WhenDisabled_ForwardedHeaders_AreIgnored()
    {
        // Arrange
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "false"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, DiagnosticEndpoint);
        request.Headers.Add("X-Forwarded-For", TestClientIp);
        request.Headers.Add("X-Forwarded-Proto", "https");

        // Act
        var info = await GetConnectionInfoAsync(request);

        // Assert - Headers should be ignored
        Assert.NotEqual(TestClientIp, info.RemoteIp);
        Assert.Equal("http", info.Scheme);
    }

    #endregion

    #region Enabled Mode - Header Processing (Permissive Mode)

    // These tests use Enabled=true WITHOUT KnownProxies, so middleware uses defaults
    // and processes all forwarded headers (permissive mode for testing)

    [Fact]
    public async Task WhenEnabled_WithoutKnownProxies_ForwardsClientIp()
    {
        // Arrange - No KnownProxies = middleware processes all requests
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, DiagnosticEndpoint);
        request.Headers.Add("X-Forwarded-For", TestClientIp);

        // Act
        var info = await GetConnectionInfoAsync(request);

        // Assert - Client IP extracted from X-Forwarded-For
        Assert.Equal(TestClientIp, info.RemoteIp);
    }

    [Fact]
    public async Task WhenEnabled_WithoutKnownProxies_ForwardsScheme()
    {
        // Arrange
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, DiagnosticEndpoint);
        request.Headers.Add("X-Forwarded-Proto", "https");

        // Act
        var info = await GetConnectionInfoAsync(request);

        // Assert - Scheme changed to https
        Assert.Equal("https", info.Scheme);
    }

    [Fact]
    public async Task WhenEnabled_WithMultipleForwardedIps_TakesFirstUntrusted()
    {
        // Arrange - Chain: client -> proxy1 -> proxy2 -> server
        // X-Forwarded-For format: "client, proxy1, proxy2" (rightmost is closest)
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:ForwardLimit"] = "1"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, DiagnosticEndpoint);
        request.Headers.Add("X-Forwarded-For", $"{TestClientIp}, {TestProxyIp}");

        // Act
        var info = await GetConnectionInfoAsync(request);

        // Assert - With limit=1, processes one hop from the right
        Assert.Equal(TestProxyIp, info.RemoteIp);
    }

    #endregion

    #region Cloudflare Mode

    [Fact]
    public async Task WhenCloudflareEnabled_UsesCfConnectingIpHeader()
    {
        // Arrange - Cloudflare mode uses CF-Connecting-IP instead of X-Forwarded-For
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:UseCloudflare"] = "true"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, DiagnosticEndpoint);
        request.Headers.Add("CF-Connecting-IP", TestClientIp);

        // Act
        var info = await GetConnectionInfoAsync(request);

        // Assert - Should use CF-Connecting-IP
        Assert.Equal(TestClientIp, info.RemoteIp);
    }

    [Fact]
    public async Task WhenCloudflareEnabled_IgnoresXForwardedFor()
    {
        // Arrange
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:UseCloudflare"] = "true"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, DiagnosticEndpoint);
        request.Headers.Add("X-Forwarded-For", TestClientIp);
        // No CF-Connecting-IP header

        // Act
        var info = await GetConnectionInfoAsync(request);

        // Assert - X-Forwarded-For ignored in Cloudflare mode
        Assert.NotEqual(TestClientIp, info.RemoteIp);
    }

    #endregion

    #region Configuration Parsing (Server Startup Tests)

    // These tests verify the server starts correctly with various proxy configurations.
    // They cannot verify actual proxy filtering because WebApplicationFactory's
    // in-memory server has no RemoteIpAddress (null), which won't match any configured proxy.

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("::1")]
    public async Task KnownProxies_ParsesValidIpAddresses_ServerStarts(string proxyIp)
    {
        // Arrange
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:KnownProxies:0"] = proxyIp
        });

        // Act
        var response = await this.httpClient!.GetAsync("/health");

        // Assert - Server starts successfully
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("10.0.0.0/8")]
    [InlineData("172.16.0.0/12")]
    [InlineData("192.168.0.0/16")]
    [InlineData("127.0.0.0/8")]
    public async Task KnownNetworks_ParsesValidCidrNotation_ServerStarts(string cidr)
    {
        // Arrange
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:KnownNetworks:0"] = cidr
        });

        // Act
        var response = await this.httpClient!.GetAsync("/health");

        // Assert - Server starts successfully
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MultipleKnownProxies_ConfiguredCorrectly_ServerStarts()
    {
        // Arrange
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:KnownProxies:0"] = "127.0.0.1",
            ["ReverseProxy:KnownProxies:1"] = "10.0.0.1",
            ["ReverseProxy:KnownProxies:2"] = "192.168.1.1"
        });

        // Act
        var response = await this.httpClient!.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AllowedHosts_ConfiguredCorrectly_ServerStarts()
    {
        // Arrange
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:AllowedHosts:0"] = "example.com",
            ["ReverseProxy:AllowedHosts:1"] = "api.example.com"
        });

        // Act
        var response = await this.httpClient!.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidIpAddress_IsIgnored_ServerStillStarts()
    {
        // Arrange - Invalid IP should be silently skipped
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:KnownProxies:0"] = "not-an-ip",
            ["ReverseProxy:KnownProxies:1"] = "127.0.0.1"
        });

        // Act
        var response = await this.httpClient!.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidCidr_IsIgnored_ServerStillStarts()
    {
        // Arrange - Invalid CIDR should be silently skipped
        CreateFactory(new Dictionary<string, string?>
        {
            ["ReverseProxy:Enabled"] = "true",
            ["ReverseProxy:KnownNetworks:0"] = "invalid-cidr",
            ["ReverseProxy:KnownNetworks:1"] = "127.0.0.0/8"
        });

        // Act
        var response = await this.httpClient!.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    private sealed record ConnectionInfo(string? RemoteIp, string Scheme, string Host);
}
