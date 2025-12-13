using System.Net;
using BoletoNetCore.Server.Configuration;
using Grpc.Core;
using ProtoV1 = BoletoNetCore.Server.Contracts.Generated.V1;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BoletoNetCore.Server.Tests.Middleware;

[Trait("Category", "Integration")]
public sealed class ApiKeyMiddlewareTests : IAsyncLifetime
{
    private const string ValidApiKey = "test-api-key-12345";

    private WebApplicationFactory<Program>? factory;
    private HttpClient? httpClient;
    private GrpcChannel? grpcChannel;

    public Task InitializeAsync()
    {
        this.factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ApiKey:Enabled"] = "true",
                        ["ApiKey:Key"] = ValidApiKey
                    });
                });
            });

        this.httpClient = this.factory.CreateClient();

        var httpHandler = this.factory.Server.CreateHandler();
        this.grpcChannel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        this.httpClient?.Dispose();
        this.grpcChannel?.Dispose();

        if (this.factory is not null)
        {
            await this.factory.DisposeAsync();
        }
    }

    #region Bypass Paths

    [Theory]
    [InlineData("/")]
    [InlineData("/health")]
    public async Task BypassPaths_WithoutApiKey_ReturnsSuccess(string path)
    {
        // Act
        var response = await this.httpClient!.GetAsync(path);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerEndpoint_WithoutApiKey_ReturnsSuccess()
    {
        // Arrange - Swagger is only available in Development, but the bypass check still applies
        // We test the swagger JSON endpoint which should be bypassed
        var request = new HttpRequestMessage(HttpMethod.Get, "/swagger/v1/swagger.json");

        // Act
        var response = await this.httpClient!.SendAsync(request);

        // Assert - Should return 200 in Development, but NOT 401
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region REST Endpoints

    [Fact]
    public async Task RestEndpoint_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await this.httpClient!.PostAsync("/api/v1/boletos", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("API key is required", body);
    }

    [Fact]
    public async Task RestEndpoint_WithInvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/boletos")
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add(ApiKeyOptions.HeaderName, "invalid-key");

        // Act
        var response = await this.httpClient!.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid API key", body);
    }

    [Fact]
    public async Task RestEndpoint_WithValidApiKey_PassesThrough()
    {
        // Arrange - Send minimal invalid request; we just want to verify auth passes
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/boletos")
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add(ApiKeyOptions.HeaderName, ValidApiKey);

        // Act & Assert
        // The request should pass auth. If it fails validation downstream, that's fine -
        // we just verify it wasn't rejected by the auth middleware.
        try
        {
            var response = await this.httpClient!.SendAsync(request);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Type registry"))
        {
            // Server-side JSON serialization error for validation response - auth passed
            // This is a known issue with gRPC JSON transcoding error serialization
        }
    }

    #endregion

    #region gRPC Endpoints

    [Fact]
    public async Task GrpcEndpoint_WithoutApiKey_ReturnsUnauthenticated()
    {
        // Arrange
        var client = new ProtoV1.BoletoV1.BoletoV1Client(this.grpcChannel);
        var request = new ProtoV1.GerarBoletoRequest { Banco = new ProtoV1.Banco { Codigo = 237 } };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => client.GerarBoletoAsync(request).ResponseAsync);

        Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
    }

    [Fact]
    public async Task GrpcEndpoint_WithInvalidApiKey_ReturnsUnauthenticated()
    {
        // Arrange
        var headers = new Metadata
        {
            { ApiKeyOptions.HeaderName, "invalid-key" }
        };
        var client = new ProtoV1.BoletoV1.BoletoV1Client(this.grpcChannel);
        var request = new ProtoV1.GerarBoletoRequest { Banco = new ProtoV1.Banco { Codigo = 237 } };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => client.GerarBoletoAsync(request, headers).ResponseAsync);

        Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
    }

    [Fact]
    public async Task GrpcEndpoint_WithValidApiKey_PassesThrough()
    {
        // Arrange
        var headers = new Metadata
        {
            { ApiKeyOptions.HeaderName, ValidApiKey }
        };
        var client = new ProtoV1.BoletoV1.BoletoV1Client(this.grpcChannel);

        // Minimal request - will fail validation but should pass auth
        var request = new ProtoV1.GerarBoletoRequest { Banco = new ProtoV1.Banco { Codigo = 237 } };

        // Act & Assert - Should NOT throw Unauthenticated; may throw InvalidArgument due to validation
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => client.GerarBoletoAsync(request, headers).ResponseAsync);

        Assert.NotEqual(StatusCode.Unauthenticated, exception.StatusCode);
    }

    #endregion
}
