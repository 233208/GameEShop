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
            }
        };
            context.Products.AddRange(Games);
            context.SaveChanges();
        }
            
    }
}
