using BoletoNetCore.Server.Contracts.Generated.V1;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BoletoNetCore.Server.IntegrationTests.Fixtures;

public sealed class BoletoGrpcTestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program>? factory;
    private GrpcChannel? channel;

    public BoletoV1.BoletoV1Client Client { get; private set; } = null!;

    public Task InitializeAsync()
    {
        this.factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            });

        var httpHandler = this.factory.Server.CreateHandler();

        this.channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        Client = new BoletoV1.BoletoV1Client(this.channel);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        this.channel?.Dispose();
        if (this.factory is not null)
        {
            await this.factory.DisposeAsync();
        }
    }
}
