using CartApplication.Producer;
using CartDomain.Repositories;
using CartService.Application.Services;
using CartService.Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;

namespace CartService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Rejestracja Us³ug ---

            // 1. Konfiguracja Magazynu Koszyka (Redis)
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                // Adres serwera Redis pobierany z docker-compose
                options.Configuration = "redis:6379";
                options.InstanceName = "Cart_";
            });

            // 2. Repozytorium i Serwis Aplikacji
            // Rejestrujemy nasze implementacje z warstwy Application
            builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
            builder.Services.AddScoped<ICartRepository, RedisCartRepository>();
            builder.Services.AddScoped<ICartService, CartService.Application.Services.CartService>();

            // 3. Konfiguracja HttpClient dla komunikacji miêdzy serwisami
            builder.Services.AddHttpClient();

            // 4. Konfiguracja API
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // 5. Konfiguracja CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    // U¿yj adresu z konfiguracji lub wartoœci domyœlnej
                    var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";
                    policy.WithOrigins(frontendUrl)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // 6. Konfiguracja Uwierzytelniania JWT
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
                options.RequireHttpsMetadata = false; // Dla œrodowiska deweloperskiego
            });

            // 7. Konfiguracja Autoryzacji
            builder.Services.AddAuthorization();

            // 8. Konfiguracja Swaggera
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cart API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Wpisz token w formacie: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            // --- Budowanie Aplikacji ---
            var app = builder.Build();

            // --- Konfiguracja Pipeline HTTP ---
            app.UseCors("CorsPolicy");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Uruchomienie aplikacji (bez migracji, bo nie ma bazy danych)
            await app.RunAsync();
        }
    }
}