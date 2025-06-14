using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Kafka;
using Testcontainers.MsSql;
using User.Domain.Repositories;
using User.Domain.Seeders;
using Xunit;

namespace UserService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();
    private readonly KafkaContainer _kafkaContainer = new KafkaBuilder().Build();

    public string KafkaBootstrapServers { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var config = new Dictionary<string, string?>
            {
                ["Kafka:BootstrapServers"] = KafkaBootstrapServers
            };
            configBuilder.AddInMemoryCollection(config);

        });


        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<UserDbContext>));
            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _kafkaContainer.StartAsync();

        var kafkaPort = _kafkaContainer.GetMappedPublicPort(KafkaBuilder.KafkaPort);
        KafkaBootstrapServers = $"{_kafkaContainer.Hostname}:{kafkaPort}";

        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<UserDbContext>();
        await dbContext.Database.MigrateAsync();

        var seeder = services.GetRequiredService<IUserSeeder>();
        await seeder.Seed();

    }
    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _kafkaContainer.StopAsync();
    }
}