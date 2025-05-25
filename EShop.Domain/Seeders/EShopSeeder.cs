using EShop.Domain.Models;
using EShop.Domain.Repositories;
namespace EShop.Domain.Seeders;

public class EShopSeeder(DataContext context) : IEShopSeeder
{
    public async Task Seed()
    {
        if (!context.Products.Any())
        {
            var Games = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "The Witcher 3: Wild Hunt",
                Description = "An open-world RPG set in a fantasy world.",
                Year = 2015,
                Genres = new[] { "RPG", "Action" },
                Price = 49.99m,
                Stock = 100,
                Producer = "CD Projekt Red",
                Publisher = "CD Projekt",
                Platform = "PC, PS4, Xbox One",
                SystemRequirements = new SystemRequirements
                {
                    OS = "Windows 10 or higher",
                    Processor = "Intel Core i5-2500K or AMD Phenom II X4 940",
                    Memory = "6 GB RAM",
                    Graphics = "NVIDIA GeForce GTX 660 or AMD Radeon HD 7870",
                    Storage = "35 GB available space"
                },
                Rating = 4.8m
                },
            new Product
            { Id = 2,
                Name = "Cyberpunk 2077",
                Description = "An open-world RPG set in a dystopian future.",
                Year = 2020,
                Genres = new[] { "RPG", "Action" },
                Price = 59.99m,
                Stock = 50,
                Producer = "CD Projekt Red",
                Publisher = "CD Projekt",
                Platform = "PC, PS4, Xbox One, PS5, Xbox Series X/S",
                SystemRequirements = new SystemRequirements
                {
                    OS = "Windows 10 or higher",
                    Processor = "Intel Core i5-3570K or AMD FX-8310",
                    Memory = "8 GB RAM",
                    Graphics = "NVIDIA GeForce GTX 780 or AMD Radeon RX 470",
                    Storage = "70 GB available space"
                },
                Rating = 4.0m
            },
            new Product
            { Id = 3,
                Name = "DOOM Eternal",
                Description = "A first-person shooter game with fast-paced action.",
                Year = 2020,
                Genres = new[] { "Shooter", "Action" },
                Price = 39.99m,
                Stock = 75,
                Producer = "id Software",
                Publisher = "Bethesda Softworks",
                Platform = "PC, PS4, Xbox One, Nintendo Switch",
                SystemRequirements = new SystemRequirements
                {
                    OS = "Windows 10",
                    Processor = "Intel Core i5-2400 or AMD Ryzen 3 1200",
                    Memory = "8 GB RAM",
                    Graphics = "NVIDIA GeForce GTX 970 or AMD Radeon R9 290",
                    Storage = "50 GB available space"
                },
                Rating = 4.7m
            }
        };
            context.Products.AddRange(Games);
            context.SaveChanges();
        }

    }
}
