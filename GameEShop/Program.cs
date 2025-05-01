using Microsoft.EntityFrameworkCore;
using EShop.Domain.Repositories;
using EShop.Application.Service;
using EShop.Domain.Seeders;

namespace EShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<DataContext>(x => x.UseInMemoryDatabase("TestDb"), ServiceLifetime.Transient);
            builder.Services.AddScoped<IRepository, Repository>();
            builder.Services.AddScoped<IEShopSeeder, EShopSeeder>();
            builder.Services.AddScoped<IProductService, ProductService>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            

            var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IEShopSeeder>();
            seeder.Seed();

            app.Run();
        }
    }
}
