using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using User.Domain.Models.Requests;
using Xunit;

namespace UserService.IntegrationTests;

public class UserEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UserEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private class LoginResponse { public string Token { get; set; } }

    [Fact]
    public async Task RegisterAndLogin_ShouldSucceed_WithValidCredentials()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Kafka:BootstrapServers"] = _factory.KafkaBootstrapServers
                });
            });
        }).CreateClient();

        var uniqueEmail = $"testuser_{Guid.NewGuid()}@example.com";
        var registerRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = uniqueEmail,
            Password = "StrongPassword123"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/register", registerRequest);

        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new LoginRequest { Username = "testuser", Password = "StrongPassword123" };

        var loginResponse = await client.PostAsJsonAsync("/api/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.NotNull(loginResult?.Token);
    }
}