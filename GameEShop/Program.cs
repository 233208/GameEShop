using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
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

            builder.Services.AddDbContext<DataContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 32)) 
                ));



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
