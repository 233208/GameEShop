using EShop.Domain.Repositories;
using EShop.Domain.Seeders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using Xunit;

namespace EShop.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<EShop.Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<DataContext>));

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<DataContext>();

        await dbContext.Database.MigrateAsync();

        var seeder = services.GetRequiredService<IEShopSeeder>();
        await seeder.Seed();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}