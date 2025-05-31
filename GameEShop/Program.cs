using EShop.Application.Service;
using EShop.Domain.Repositories;
using EShop.Domain.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;
using User.Domain.Repositories;

namespace EShop
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Tworzenie buildera aplikacji webowej
            var builder = WebApplication.CreateBuilder(args);

            // Konfiguracja kontekstu bazy danych z u¿yciem SQL Server
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            // Rejestracja repozytorium i seederów w kontenerze DI
            builder.Services.AddScoped<IRepository, Repository>();
            builder.Services.AddScoped<IEShopSeeder, EShopSeeder>();
            builder.Services.AddScoped<IProductService, ProductService>();

            // Dodanie kontrolerów do kontenera us³ug
            builder.Services.AddControllers();

            // Konfiguracja Swaggera do dokumentacji API
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Wpisz token w formacie: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            },
                            new List<string>()
                        }
                });
            });

            // Konfiguracja uwierzytelniania JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(File.ReadAllText("./data/public.key"));
                var publicKey = new RsaSecurityKey(rsa);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "GameEShop",
                    ValidAudience = "Eshop",
                    IssuerSigningKey = publicKey
                };
            });

            // Konfiguracja autoryzacji z politykami ról
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("EmployeeOnly", policy =>
                    policy.RequireRole("Employee"));
            });

            // Konfiguracja CORS dla okreœlonego pochodzenia
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:64452")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Budowanie aplikacji
            var app = builder.Build();

            // U¿ycie polityki CORS
            app.UseCors("CorsPolicy");

            // W³¹czenie Swaggera w œrodowisku deweloperskim
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Middleware uwierzytelniania i autoryzacji
            app.UseAuthentication();
            app.UseAuthorization();

            // Mapowanie kontrolerów
            app.MapControllers();

            // Migracja bazy danych i uruchomienie seeda
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                await db.Database.MigrateAsync();
                var seeder = scope.ServiceProvider.GetRequiredService<IEShopSeeder>();
                await seeder.Seed();
            }

            // Uruchomienie aplikacji
            await app.RunAsync();
        }
    }
}
